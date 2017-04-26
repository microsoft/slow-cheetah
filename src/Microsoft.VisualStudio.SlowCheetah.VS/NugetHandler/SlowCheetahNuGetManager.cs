// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnvDTE;
    using Microsoft.Build.Construction;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Manages installations of the SlowCheetah NuGet package in the project
    /// </summary>
    public class SlowCheetahNuGetManager
    {
        private static readonly string PackageName = "Microsoft.VisualStudio.SlowCheetah";
        private static readonly string OldPackageName = "SlowCheetah";
        private static readonly Version LastUnsupportedVersion = new Version(2, 5, 15);

        // Fields for checking NuGet support
        private static readonly Guid INuGetPackageManagerGuid = Guid.Parse("FD2DC07E-9054-4115-B86B-26A9F9C1F00B");
        private static readonly string SupportedCapabilities = "AssemblyReferences + DeclaredSourceItems + UserSourceItems";
        private static readonly string UnsupportedCapabilities = "SharedAssetsProject";
        private static readonly HashSet<string> SupportedProjectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VsProjectTypes.WebSiteProjectTypeGuid,
                VsProjectTypes.CsharpProjectTypeGuid,
                VsProjectTypes.VbProjectTypeGuid,
                VsProjectTypes.CppProjectTypeGuid,
                VsProjectTypes.JsProjectTypeGuid,
                VsProjectTypes.FsharpProjectTypeGuid,
                VsProjectTypes.NemerleProjectTypeGuid,
                VsProjectTypes.WixProjectTypeGuid,
                VsProjectTypes.SynergexProjectTypeGuid,
                VsProjectTypes.NomadForVisualStudioProjectTypeGuid,
                VsProjectTypes.TDSProjectTypeGuid,
                VsProjectTypes.DxJsProjectTypeGuid,
                VsProjectTypes.DeploymentProjectTypeGuid,
                VsProjectTypes.CosmosProjectTypeGuid,
                VsProjectTypes.ManagementPackProjectTypeGuid,
            };

        private readonly IServiceProvider package;

        private readonly HashSet<string> installTasks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object syncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahNuGetManager"/> class.
        /// </summary>
        /// <param name="package">VS Package</param>
        public SlowCheetahNuGetManager(IServiceProvider package)
        {
            this.package = package;
        }

        /// <summary>
        /// Checks if the current project supports NuGet
        /// </summary>
        /// <param name="hierarchy">Hierarchy of the project to be verified</param>
        /// <returns>True if the project supports NuGet</returns>
        /// <remarks>This implementation is derived of the internal NuGet method IsSupported
        /// https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Utility/EnvDTEProjectUtility.cs#L441
        /// This should be removed when NuGet adds this to their public API</remarks>
        public bool ProjectSupportsNuget(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            if (SupportsINugetProjectSystem(hierarchy))
            {
                return true;
            }

            try
            {
                if (hierarchy.IsCapabilityMatch(SupportedCapabilities))
                {
                    return true;
                }

                if (hierarchy.IsCapabilityMatch(UnsupportedCapabilities))
                {
                    return false;
                }
            }
            catch (Exception ex) when (!ErrorHandler.IsCriticalException(ex))
            {
                // Catch exceptions when hierarchy doesn't support the above methods
            }

            Project project = PackageUtilities.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);

            return project.Kind != null && SupportedProjectTypes.Contains(project.Kind);
        }

        /// <summary>
        /// Checks the SlowCheetah NuGet package on current project.
        /// If no version is installed, prompts for install of latest version;
        /// if an older version is detected, shows update information.
        /// </summary>
        /// <param name="hierarchy">Hierarchy of the project to be verified</param>
        public void CheckSlowCheetahInstallation(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            Project currentProject = PackageUtilities.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);

            // Whether or not an even older version of SlowCheetah (before NuGet) is installed
            // or some old code is still present in the project file
            bool isOldScInstalled = IsOldSlowCheetahInstalled(hierarchy as IVsBuildPropertyStorage);
            if (isOldScInstalled)
            {
                // Delete the older code and install the latest NuGet package
                this.UpdateSlowCheetah(currentProject);
            }
            else
            {
                // If there's no old code, update the package if it is still SlowCheetah
                // and not Microsoft.VisualStudio.SlowCheetah
                if (!this.IsMicrosoftPackageInstalled(currentProject))
                {
                    if (!this.IsSlowCheetahUpdated(currentProject))
                    {
                        // If the package is 2.5.15 or earlier, the user's project file
                        // probably contains older targets, so show info on removing these
                        INugetPackageHandler nugetHandler = NugetHandlerFactory.GetHandler(this.package);
                        nugetHandler.ShowUpdateInfo();
                    }

                    // Install the newest SlowCheetah package
                    // If the older package (SlowCheetah) is installed,
                    // uninstall it to make way for Microsoft.VisualStudio.SlowCheetah
                    this.BackgroundInstallSlowCheetah(currentProject, this.IsOldSlowCheetahPackageInstalled(currentProject));
                }
            }
        }

        private static IVsPackageInstallerServices GetInstallerServices(IServiceProvider package)
        {
            var componentModel = (IComponentModel)package.GetService(typeof(SComponentModel));
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            return installerServices;
        }

        private static bool IsOldSlowCheetahInstalled(IVsBuildPropertyStorage buildPropertyStorage)
        {
            buildPropertyStorage.GetPropertyValue("SlowCheetahImport", null, (uint)_PersistStorageType.PST_PROJECT_FILE, out string propertyValue);
            if (!string.IsNullOrEmpty(propertyValue))
            {
                return true;
            }

            buildPropertyStorage.GetPropertyValue("SlowCheetahTargets", null, (uint)_PersistStorageType.PST_PROJECT_FILE, out propertyValue);
            if (!string.IsNullOrEmpty(propertyValue))
            {
                return true;
            }

            return false;
        }

        private static bool SupportsINugetProjectSystem(IVsHierarchy hierarchy)
        {
            var vsProject = hierarchy as IVsProject;
            if (vsProject == null)
            {
                return false;
            }

            vsProject.GetItemContext(
                (uint)VSConstants.VSITEMID.Root,
                out Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider);
            if (serviceProvider == null)
            {
                return false;
            }

            using (var sp = new ServiceProvider(serviceProvider))
            {
                var retValue = sp.GetService(INuGetPackageManagerGuid);
                return retValue != null;
            }
        }

        private bool IsMicrosoftPackageInstalled(Project project)
        {
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);
            return installerServices.IsPackageInstalled(project, PackageName);
        }

        /// <summary>
        /// Checks if the SlowCheetah package is installed, <see cref="OldPackageName"/>
        /// The newest version of the package is given by <see cref="PackageName"/>
        /// </summary>
        /// <param name="project">The current project</param>
        /// <returns>true if the older SlowCheetah package is installed</returns>
        private bool IsOldSlowCheetahPackageInstalled(Project project)
        {
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);
            return installerServices.IsPackageInstalled(project, OldPackageName);
        }

        private bool IsSlowCheetahUpdated(Project project)
        {
            // Checks for older SC versions that require more complex update procedure.
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);

            if (installerServices.IsPackageInstalled(project, PackageName))
            {
                return true;
            }

            IVsPackageMetadata scPackage =
                    installerServices.GetInstalledPackages().FirstOrDefault(pkg => string.Equals(pkg.Id, OldPackageName, StringComparison.OrdinalIgnoreCase));
            if (scPackage != null)
            {
                if (Version.TryParse(scPackage.VersionString, out Version ver))
                {
                    return ver > LastUnsupportedVersion;
                }
            }

            return false;
        }

        private bool HasUserAcceptedWarningMessage(string title, string message)
        {
            if (this.package.GetService(typeof(SVsUIShell)) is IVsUIShell shell)
            {
                // Show a yes or no message box with the given title and message
                Guid compClass = Guid.Empty;
                if (ErrorHandler.Succeeded(shell.ShowMessageBox(0, ref compClass, title, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 1, out int result)))
                {
                    return result == (int)VSConstants.MessageBoxResult.IDYES;
                }
            }

            return false;
        }

        private void BackgroundInstallSlowCheetah(Project project, bool uninstallOldPackage)
        {
            string projName = project.UniqueName;
            bool needInstall = true;
            lock (this.syncObject)
            {
                needInstall = this.installTasks.Add(projName);
            }

            if (needInstall)
            {
                if (this.HasUserAcceptedWarningMessage(Resources.Resources.NugetInstall_Title, Resources.Resources.NugetInstall_Text))
                {
                    // Gets the general output pane to inform user of installation
                    IVsOutputWindowPane outputWindow = (IVsOutputWindowPane)this.package.GetService(typeof(SVsGeneralOutputWindowPane));
                    outputWindow?.OutputString(string.Format(Resources.Resources.NugetInstall_InstallingOutput, project.Name) + Environment.NewLine);

                    // Uninstalls the older version (if present) and installs latest package
                    var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                    IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();
                    IVsPackageUninstaller packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();

                    TPL.Task.Run(() =>
                    {
                        string outputMessage = Resources.Resources.NugetInstall_FinishedOutput;
                        try
                        {
                            if (uninstallOldPackage)
                            {
                                packageUninstaller.UninstallPackage(project, OldPackageName, true);
                            }

                            packageInstaller.InstallLatestPackage(null, project, PackageName, false, false);
                        }
                        catch
                        {
                            outputMessage = Resources.Resources.NugetInstall_ErrorOutput;
                            throw;
                        }
                        finally
                        {
                            lock (this.syncObject)
                            {
                                this.installTasks.Remove(projName);
                            }

                            ThreadHelper.Generic.BeginInvoke(() => outputWindow?.OutputString(string.Format(outputMessage, project.Name) + Environment.NewLine));
                        }
                    });
                }
                else
                {
                    lock (this.syncObject)
                    {
                        // If the user refuses to install, the task should not be added
                        this.installTasks.Remove(projName);
                    }
                }
            }
        }

        private void UpdateSlowCheetah(Project project)
        {
            // This is done on the UI thread because changes are made to the project file,
            // causing it to be reloaded. To avoid conflicts with NuGet installation,
            // the update is done sequentially
            if (this.HasUserAcceptedWarningMessage(Resources.Resources.NugetUpdate_Title, Resources.Resources.NugetUpdate_Text))
            {
                // Creates dialog informing the user to wait for the installation to finish
                IVsThreadedWaitDialogFactory twdFactory = this.package.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                IVsThreadedWaitDialog2 dialog = null;
                twdFactory?.CreateInstance(out dialog);

                string title = Resources.Resources.NugetUpdate_WaitTitle;
                string text = Resources.Resources.NugetUpdate_WaitText;
                dialog?.StartWaitDialog(title, text, null, null, null, 0, false, true);
                try
                {
                    // Installs the latest version of the SlowCheetah NuGet package
                    var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                    if (this.IsOldSlowCheetahPackageInstalled(project))
                    {
                        IVsPackageUninstaller packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();
                        packageUninstaller.UninstallPackage(project, OldPackageName, true);
                    }

                    IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();
                    packageInstaller.InstallLatestPackage(null, project, PackageName, false, false);

                    project.Save();
                    ProjectRootElement projectRoot = ProjectRootElement.Open(project.FullName);
                    foreach (ProjectPropertyGroupElement propertyGroup in projectRoot.PropertyGroups.Where(pg => pg.Label.Equals("SlowCheetah")))
                    {
                        projectRoot.RemoveChild(propertyGroup);
                    }

                    foreach (ProjectImportElement import in projectRoot.Imports.Where(i => i.Label == "SlowCheetah" || i.Project == "$(SlowCheetahTargets)"))
                    {
                        projectRoot.RemoveChild(import);
                    }

                    projectRoot.Save();
                }
                finally
                {
                    // Closes the wait dialog. If the dialog failed, does nothing
                    dialog?.EndWaitDialog(out int canceled);
                }
            }
        }
    }
}
