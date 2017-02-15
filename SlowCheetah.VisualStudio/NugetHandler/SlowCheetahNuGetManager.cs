// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;
    using System.Linq;
    using EnvDTE;
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

        private IServiceProvider package;

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
        /// <param name="project">Project to be verified</param>
        public void CheckSlowCheetahInstallation(Project project)
        {
            if (this.IsSlowCheetahInstalled(project))
            {
                if (!this.IsSlowCheetahUpdated(project))
                {
                    INugetPackageHandler nugetHandler = NugetHandlerFactory.GetHandler(this.package);
                    nugetHandler.ShowUpdateInfo();
                }
            }
            else
            {
                this.InstallSlowCheetah(project);
            }
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
            if (this.HasUserAcceptedWarningMessage())
            {
                // Creates dialog informing the user to wait for the installation to finish
                IVsThreadedWaitDialogFactory twdFactory = this.package.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                IVsThreadedWaitDialog2 dialog = null;
                twdFactory?.CreateInstance(out dialog);
                string title = Resources.Resources.NugetInstall_WaitTitle;
                string text = Resources.Resources.NugetInstall_WaitText;
                dialog?.StartWaitDialog(title, text, null, null, null, 0, false, true);

                // Installs the latest version of the SlowCheetah NuGet package
                var componentModel = (IComponentModel)this.package.GetService(typeof(SComponentModel));
                IVsPackageInstaller2 packageInstaller = componentModel.GetService<IVsPackageInstaller2>();
                packageInstaller.InstallLatestPackage(null, project, PackageName, false, false);

                // Closes the wait dialog. If the dialog failed, does nothing
                int canceled;
                dialog?.EndWaitDialog(out canceled);
            }
        }

        private bool HasUserAcceptedWarningMessage()
        {
            IVsUIShell shell = this.package.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
            {
                // Show a message box requesting the install of the SC package
                string title = Resources.Resources.NugetInstall_Title;
                string message = Resources.Resources.NugetInstall_Text;
                Guid compClass = Guid.Empty;
                int result;
                if (ErrorHandler.Succeeded(shell.ShowMessageBox(0, ref compClass, title, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 1, out result)))
                {
                    return result == 6; // IDYES
                }
            }

            return false;
        }
    }
}
