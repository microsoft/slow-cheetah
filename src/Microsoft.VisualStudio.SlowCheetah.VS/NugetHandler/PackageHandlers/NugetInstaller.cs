// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using EnvDTE;
    using Microsoft;
    using Microsoft.VisualStudio.ComponentModelHost;
    using NuGet.VisualStudio;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Installs the latest SlowCheetah NuGet package
    /// </summary>
    internal class NugetInstaller : BasePackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetInstaller"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public NugetInstaller(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <inheritdoc/>
        public override async TPL.Task ExecuteAsync(Project project)
        {
            var componentModel = (IComponentModel)await this.Package.GetServiceAsync(typeof(SComponentModel));
            Assumes.Present(componentModel);
            IVsPackageInstaller packageInstaller = componentModel.GetService<IVsPackageInstaller>();
            packageInstaller.InstallPackage(
                null,
                project,
                SlowCheetahNuGetManager.PackageName,
                version: (string)null, // install latest stable version
                ignoreDependencies: false);

            await this.Successor.ExecuteAsync(project);
        }
    }
}
