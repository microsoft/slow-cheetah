// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using EnvDTE;
    using Microsoft.Build.Construction;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using SlowCheetah.VisualStudio.Properties;

    /// <summary>
    /// Manages installations of the SlowCheetah NuGet package in the project
    /// </summary>
    public class SlowCheetahNuGetManager
    {
        private static readonly string PackageName = Settings.Default.SlowCheetahNugetPkgName;
        private static readonly int InstallDialogDelay = 1;

        private static SlowCheetahNuGetManager instance = null;

        private IServiceProvider package;

        private ConcurrentDictionary<string, Task> installTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahNuGetManager"/> class.
        /// </summary>
        /// <param name="package">VS Package</param>
        private SlowCheetahNuGetManager(IServiceProvider package)
        {
            this.package = package;
            this.installTasks = new ConcurrentDictionary<string, Task>();
        }

        /// <summary>
        /// Gets the instance of <see cref="SlowCheetahNuGetManager"/>
        /// </summary>
        /// <param name="package">VS Package</param>
        /// <returns>Singleton instance of this manager</returns>
        public static SlowCheetahNuGetManager GetInstance(IServiceProvider package)
        {
            if (instance == null)
            {
                instance = new SlowCheetahNuGetManager(package);
            }

            instance.package = package;
            return instance;
        }

        /// <summary>
        /// Checks the SlowCheetah NuGet package on current project.
        /// If no version is installed, prompts for install of latest version;
        /// if an older version is detected, shows update information.
        /// </summary>
        /// <param name="hierarchy">Hierarchy of the project to be verified</param>
        public void CheckSlowCheetahInstallation(IVsHierarchy hierarchy)
        {
            Project currentProject = PackageUtilities.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);
            bool isOldScInstalled = this.IsOldSlowCheetahInstalled(hierarchy as IVsBuildPropertyStorage);
            if (isOldScInstalled || this.IsSlowCheetahInstalled(currentProject))
            {
                if (isOldScInstalled)
                {
                    this.UpdateOldSlowCheetah(currentProject);
                }
                else if (!this.IsSlowCheetahUpdated(currentProject))
                {
                    INugetPackageHandler nugetHandler = NugetHandlerFactory.GetHandler(this.package);
                    nugetHandler.ShowUpdateInfo();
                }
            }
            else
            {
                this.BackgroundInstallSlowCheetah(currentProject);
            }
        }

        private static void InstallSlowCheetahPackage(IVsPackageInstaller2 installer, Project project)
        {
            installer.InstallLatestPackage(null, project, PackageName, false, false);
        }

        private bool IsOldSlowCheetahInstalled(IVsBuildPropertyStorage buildPropertyStorage)
        {
            string propertyValue;
            buildPropertyStorage.GetPropertyValue("SlowCheetahImport", null, (uint)_PersistStorageType.PST_PROJECT_FILE, out propertyValue);
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
            var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            return installerServices.IsPackageInstalled(project, PackageName);
        }

        private bool IsSlowCheetahUpdated(Project project)
        {
            // Checks for older SC versions that require more complex update procedure
            var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            IVsPackageMetadata scPackage =
                    installerServices.GetInstalledPackages().First(pkg => string.Equals(pkg.Id, PackageName, StringComparison.OrdinalIgnoreCase));
            if (scPackage != null)
            {
                Version ver;
                if (Version.TryParse(scPackage.VersionString, out ver))
                {
                    return ver > new Version(2, 5, 15);
                }
            }

            return false;
        }

        private void InstallSlowCheetah(Project project)
        {
            if (this.HasUserAcceptedWarningMessage(Resources.Resources.NugetInstall_Title, Resources.Resources.NugetInstall_Title))
            {
                // Creates dialog informing the user to wait for the installation to finish
                IVsThreadedWaitDialogFactory twdFactory = this.package.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                IVsThreadedWaitDialog2 dialog = null;
                twdFactory?.CreateInstance(out dialog);
                string title = Resources.Resources.NugetInstall_WaitTitle;
                string text = Resources.Resources.NugetInstall_WaitText;
                dialog?.StartWaitDialog(title, text, null, null, null, InstallDialogDelay, false, true);

                // Installs the latest version of the SlowCheetah NuGet package
                var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();
                packageInstaller.InstallLatestPackage(null, project, PackageName, false, false);

                // Closes the wait dialog. If the dialog failed, does nothing
                int canceled;
                dialog?.EndWaitDialog(out canceled);
            }
        }

        private void BackgroundInstallSlowCheetah(Project project)
        {
            string projName = project.UniqueName;
            Task installTask;
            if (!this.installTasks.TryGetValue(projName, out installTask))
            {
                if (this.HasUserAcceptedWarningMessage(Resources.Resources.NugetInstall_Title, Resources.Resources.NugetInstall_Title))
                {
                    // Installs the latest version of the SlowCheetah NuGet package
                    var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                    IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();
                    installTask = Task.Run(() =>
                    {
                        InstallSlowCheetahPackage(packageInstaller, project);
                    }).ContinueWith(t =>
                    {
                        Task outTask;
                        this.installTasks.TryRemove(projName, out outTask);
                    });
                    this.installTasks.TryAdd(projName, installTask);
                }
            }
        }

        private bool HasUserAcceptedWarningMessage(string title, string message)
        {
            IVsUIShell shell = this.package.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
            {
                // Show a yes or no message box with the given title and message
                Guid compClass = Guid.Empty;
                int result;
                if (ErrorHandler.Succeeded(shell.ShowMessageBox(0, ref compClass, title, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 1, out result)))
                {
                    return result == 6; // IDYES
                }
            }

            return false;
        }

        private void UpdateOldSlowCheetah(Project project)
        {
            if (this.HasUserAcceptedWarningMessage("Update?", "This will edit your project file!!1!"))
            {
                ProjectRootElement projectRoot = ProjectRootElement.Open(project.FullName);
                foreach (ProjectPropertyGroupElement propertyGroup in projectRoot.PropertyGroups.Where(pg => pg.Label.Equals("SlowCheetah")))
                {
                    projectRoot.RemoveChild(propertyGroup);
                }

                foreach (ProjectImportElement import in projectRoot.Imports.Where(i => i.Label == "SlowCheetah" || i.Project == "$(SlowCheetahTargets)"))
                {
                    projectRoot.RemoveChild(import);
                }

                // Installs the latest version of the SlowCheetah NuGet package
                var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                IVsPackageUninstaller packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();
                IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();

                packageUninstaller.UninstallPackage(project, PackageName, true);
                packageInstaller.InstallLatestPackage(null, project, PackageName, false, false);
            }
        }
    }
}
