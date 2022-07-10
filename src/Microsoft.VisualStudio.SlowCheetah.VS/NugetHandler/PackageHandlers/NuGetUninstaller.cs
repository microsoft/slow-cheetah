// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using EnvDTE;
    using Microsoft;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using NuGet.VisualStudio;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Uninstalls older versions of the SlowCheetah NuGet package
    /// </summary>
    internal class NuGetUninstaller : BasePackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetUninstaller"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public NuGetUninstaller(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <inheritdoc/>
        public override async TPL.Task Execute(Project project)
        {
            var componentModel = (IComponentModel)await this.Package.GetServiceAsync(typeof(SComponentModel));
            Assumes.Present(componentModel);
            IVsPackageUninstaller packageUninstaller = componentModel.GetService<IVsPackageUninstaller>();
            packageUninstaller.UninstallPackage(project, SlowCheetahNuGetManager.OldPackageName, true);

            await this.Successor.Execute(project);
        }
    }
}
