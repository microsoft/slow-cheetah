// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using EnvDTE;
    using Microsoft;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using NuGet.VisualStudio.Contracts;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Manages installations of the SlowCheetah NuGet package in the project
    /// </summary>
    public class SlowCheetahNuGetManager
    {
        /// <summary>
        /// The name of the SlowCheetah NuGet package
        /// </summary>
        internal static readonly string PackageName = "Microsoft.VisualStudio.SlowCheetah";

        /// <summary>
        /// The previous name of the SlowCheetah NuGet package
        /// </summary>
        internal static readonly string OldPackageName = "SlowCheetah";

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

        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahNuGetManager"/> class.
        /// </summary>
        /// <param name="package">VS Package</param>
        public SlowCheetahNuGetManager(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Checks if the current project supports NuGet
        /// </summary>
        /// <param name="hierarchy">Hierarchy of the project to be verified</param>
        /// <returns>True if the project supports NuGet</returns>
        /// <remarks>This implementation is derived of the internal NuGet method IsSupported
        /// https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Utility/EnvDTEProjectUtility.cs#L441
        /// This should be removed when NuGet adds this to their public API.</remarks>
        public bool ProjectSupportsNuget(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
        /// <returns>A <see cref="TPL.Task"/> representing the asynchronous operation.</returns>
        public async TPL.Task CheckSlowCheetahInstallationAsync(IVsHierarchy hierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            Project currentProject = PackageUtilities.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);

            // Whether an older version of SlowCheetah is installed
            // through manual imports in the user's project file
            bool isOldScInstalled = IsOldSlowCheetahInstalled(hierarchy as IVsBuildPropertyStorage);

            // Wheter the old NuGet package is installed
            bool isOldScPackageInstalled = this.IsPackageInstalled(currentProject, OldPackageName);

            // Wether the newest NuGet package is installed
            bool isNewScPackageInstalled = this.IsPackageInstalled(currentProject, PackageName);

            IPackageHandler plan = new EmptyHandler(this.package);

            if (!isNewScPackageInstalled)
            {
                // If the new package is not present, it will need to be installed
                plan = new NugetInstaller(plan);
            }

            if (isOldScPackageInstalled)
            {
                // If the old package is present, it will need to be uninstalled
                plan = new NuGetUninstaller(plan);
            }

            if (isOldScInstalled)
            {
                // If the older targets are installed, they need to be removed
                // This needs to be done through a wait dialog since the project file will be altered
                plan = new TargetsUninstaller(plan);
                plan = new DialogInstallationHandler(plan);
            }
            else if (!(plan is EmptyHandler))
            {
                // If there are actions to execute and no targets are found,
                // perform these actions in the background
                plan = new BackgroundInstallationHandler(plan)
                {
                    // If the old package is installed, this is an update operation
                    IsUpdate = isOldScPackageInstalled,
                };
            }

            await plan.Execute(currentProject);
        }

        private static IVsPackageInstallerServices GetInstallerServices(IServiceProvider package)
        {
            var componentModel = (IComponentModel)package.GetService(typeof(SComponentModel));
            Assumes.Present(componentModel);
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            return installerServices;
        }

        private static bool IsOldSlowCheetahInstalled(IVsBuildPropertyStorage buildPropertyStorage)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
            ThreadHelper.ThrowIfNotOnUIThread();
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

        private bool IsPackageInstalled(Project project, string packageName)
        {
            IVsPackageInstallerServices installerServices = GetInstallerServices(this.package);
            return installerServices.IsPackageInstalled(project, packageName);
        }
    }
}
