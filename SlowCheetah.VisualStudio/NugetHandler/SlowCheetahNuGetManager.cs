// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
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
    using SlowCheetah.VisualStudio.Properties;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Manages installations of the SlowCheetah NuGet package in the project
    /// </summary>
    public class SlowCheetahNuGetManager
    {
        private static readonly string PackageName = Settings.Default.SlowCheetahNugetPkgName;
        private static readonly Version LastUnsupportedVersion = new Version(2, 5, 15);

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
            bool isOldScInstalled = IsOldSlowCheetahInstalled(hierarchy as IVsBuildPropertyStorage);

            if (isOldScInstalled)
            {
                this.UpdateSlowCheetah(currentProject);
            }
            else if (!this.IsSlowCheetahInstalled(currentProject))
            {
                // If SlowCheetah is not installed at all
                this.BackgroundInstallSlowCheetah(currentProject);
            }
            else if (!this.IsSlowCheetahUpdated(currentProject))
            {
                // In this case, an older NuGet package is installed,
                // but traces of old SlowCheetah installation were not found
                // This means the user may have manually edited their project file.
                // In this case, show the update information so that they know the proper way to uninstall
                INugetPackageHandler nugetHandler = NugetHandlerFactory.GetHandler(this.package);
                nugetHandler.ShowUpdateInfo();
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

        private bool IsSlowCheetahInstalled(Project project)
        {
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);
            return installerServices.IsPackageInstalled(project, PackageName);
        }

        private bool IsSlowCheetahUpdated(Project project)
        {
            // Checks for older SC versions that require more complex update procedure.
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);
            IVsPackageMetadata scPackage =
                    installerServices.GetInstalledPackages().FirstOrDefault(pkg => string.Equals(pkg.Id, PackageName, StringComparison.OrdinalIgnoreCase));
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

        private void BackgroundInstallSlowCheetah(Project project)
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

                    TPL.Task.Run(() =>
                    {
                        string outputMessage = Resources.Resources.NugetInstall_FinishedOutput;
                        try
                        {
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
                    if (this.IsSlowCheetahInstalled(project))
                    {
                        IVsPackageUninstaller packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();
                        packageUninstaller.UninstallPackage(project, PackageName, true);
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
