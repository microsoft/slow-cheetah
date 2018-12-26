// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

    // This attribute is used to register the informations needed to show the this package in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.GuidSlowCheetahPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}", PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionsDialogPage), "Slow Cheetah", "General", 100, 101, true)]
    [ProvideOptionPage(typeof(AdvancedOptionsDialogPage), "Slow Cheetah", "Advanced", 100, 101, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed partial class SlowCheetahPackage : AsyncPackage
    {
        /// <summary>
        /// The TransformOnBuild metadata name
        /// </summary>
        public static readonly string TransformOnBuild = "TransformOnBuild";

        /// <summary>
        /// The IsTransformFile metadata name
        /// </summary>
        public static readonly string IsTransformFile = "IsTransformFile";

        /// <summary>
        /// The DependentUpon metadata name
        /// </summary>
        public static readonly string DependentUpon = "DependentUpon";

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahPackage"/> class.
        /// </summary>
        public SlowCheetahPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            OurPackage = this;
        }

        /// <summary>
        /// Gets the SlowCheetahPackage
        /// </summary>
        public static SlowCheetahPackage OurPackage { get; private set; }

        private SlowCheetahNuGetManager NuGetManager { get; set; }

        private SlowCheetahPackageLogger PackageLogger { get; set; }

        private ErrorListProvider ErrorListProvider { get; set; }

        private AddTransformCommand AddCommand { get; set; }

        private PreviewTransformCommand PreviewCommand { get; set; }

        private PackageSolutionEvents SolutionEvents { get; set; }

        /// <summary>
        /// Verifies if the current project supports transformations.
        /// </summary>
        /// <param name="project">Current IVsProject</param>
        /// <returns>True if the project supports transformation</returns>
        public bool ProjectSupportsTransforms(IVsProject project)
        {
            return this.NuGetManager.ProjectSupportsNuget(project as IVsHierarchy);
        }

        /// <summary>
        /// Verifies if the item has a trasform configured already
        /// </summary>
        /// <param name="vsProject">The current project</param>
        /// <param name="itemid">The id of the selected item inside the project</param>
        /// <returns>True if the item has a transform</returns>
        public bool IsItemTransformItem(IVsProject vsProject, uint itemid)
        {
            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null)
            {
                this.PackageLogger.LogMessage("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                return false;
            }

            buildPropertyStorage.GetItemAttribute(itemid, IsTransformFile, out string value);
            if (bool.TryParse(value, out bool valueAsBool) && valueAsBool)
            {
                return true;
            }

            // we need to special case web.config transform files
            buildPropertyStorage.GetItemAttribute(itemid, "FullPath", out string filePath);
            IEnumerable<string> configs = ProjectUtilities.GetProjectConfigurations(vsProject as IVsHierarchy);

            // If the project is a web app, check for the Web.config files added by default
            return ProjectUtilities.IsProjectWebApp(vsProject) && PackageUtilities.IsFileTransformForBuildConfiguration("web.config", Path.GetFileName(filePath), configs);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Package load progress provider.</param>
        /// <returns>Async task.</returns>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            this.NuGetManager = new SlowCheetahNuGetManager(this);
            this.PackageLogger = new SlowCheetahPackageLogger(this);
            this.ErrorListProvider = new ErrorListProvider(this);
            this.AddCommand = new AddTransformCommand(this, this.NuGetManager, this.PackageLogger);
            this.PreviewCommand = new PreviewTransformCommand(this, this.NuGetManager, this.PackageLogger, this.ErrorListProvider);
            this.SolutionEvents = new PackageSolutionEvents(this, this.ErrorListProvider);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.SolutionEvents.Dispose();

                this.PreviewCommand.Dispose();

                base.Dispose(disposing);
            }
        }
    }
}
