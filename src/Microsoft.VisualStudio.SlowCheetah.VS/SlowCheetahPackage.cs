// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
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
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // This attribute is used to register the informations needed to show the this package in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.GuidSlowCheetahPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideOptionPage(typeof(OptionsDialogPage), "Slow Cheetah", "General", 100, 101, true)]
    [ProvideOptionPage(typeof(AdvancedOptionsDialogPage), "Slow Cheetah", "Advanced", 100, 101, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed partial class SlowCheetahPackage : Package, IVsUpdateSolutionEvents
    {
        private static readonly string TransformOnBuild = "TransformOnBuild";
        private static readonly string IsTransformFile = "IsTransformFile";

        private ErrorListProvider errorListProvider;

        private uint solutionUpdateCookie = 0;

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
            this.NuGetManager = new SlowCheetahNuGetManager(this);
        }

        /// <summary>
        /// Gets the SlowCheetahPackage
        /// </summary>
        public static SlowCheetahPackage OurPackage { get; private set; }

        private IList<string> TempFilesCreated { get; } = new List<string>();

        private SlowCheetahNuGetManager NuGetManager { get; }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.LogMessageWriteLineFormat("SlowCheetah initalizing");

            // Initialization logic
            this.errorListProvider = new ErrorListProvider(this);
            IVsSolutionBuildManager solutionBuildManager = this.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            solutionBuildManager.AdviseUpdateSolutionEvents(this, out this.solutionUpdateCookie);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            if (this.GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                // create the command for the "Add Transform" query status menu item
                CommandID menuContextCommandID = new CommandID(Guids.GuidSlowCheetahCmdSet, (int)PkgCmdID.CmdIdAddTransform);
                OleMenuCommand menuCommand = new OleMenuCommand(this.OnAddTransformCommand, this.OnChangeAddTransformMenu, this.OnBeforeQueryStatusAddTransformCommand, menuContextCommandID);
                mcs.AddCommand(menuCommand);

                // create the command for the Preview Transform menu item
                menuContextCommandID = new CommandID(Guids.GuidSlowCheetahCmdSet, (int)PkgCmdID.CmdIdPreviewTransform);
                menuCommand = new OleMenuCommand(this.OnPreviewTransformCommand, this.OnChangePreviewTransformMenu, this.OnBeforeQueryStatusPreviewTransformCommand, menuContextCommandID);
                mcs.AddCommand(menuCommand);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            foreach (string file in this.TempFilesCreated)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    this.LogMessageWriteLineFormat(
                        "There was an error deleting a temp file [{0}], error: [{1}]",
                        file,
                        ex.Message);
                }
            }

            if (this.solutionUpdateCookie > 0)
            {
                IVsSolutionBuildManager solutionBuildManager = this.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
                solutionBuildManager.UnadviseUpdateSolutionEvents(this.solutionUpdateCookie);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Verifies if the item has a trasform configured already
        /// </summary>
        /// <param name="vsProject">The current project</param>
        /// <param name="itemid">The id of the selected item inside the project</param>
        /// <returns>True if the item has a transform</returns>
        private bool IsItemTransformItem(IVsProject vsProject, uint itemid)
        {
            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null)
            {
                this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
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
            return ProjectUtilities.IsProjectWebApp(vsProject) && PackageUtilities.IsFileTransform("web.config", Path.GetFileName(filePath), configs);
        }

        /// <summary>
        /// Verifies if the current project supports transformations.
        /// </summary>
        /// <param name="project">Current IVsProject</param>
        /// <returns>True if the project supports transformation</returns>
        private bool ProjectSupportsTransforms(IVsProject project)
        {
            return this.NuGetManager.ProjectSupportsNuget(project as IVsHierarchy);
        }

        /// <summary>
        /// Gets the error info set on the thread. Returns empty string is none set (not null)
        /// </summary>
        /// <returns>Error info</returns>
        private string GetErrorInfo()
        {
            string errText = null;
            IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService(typeof(IVsUIShell));
            if (uiShell != null)
            {
                uiShell.GetErrorInfo(out errText);
            }

            if (errText == null)
            {
                return string.Empty;
            }

            return errText;
        }

        private void LogMessageWriteLineFormat(string message, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string fullMessage = string.Format(message, args);
            Trace.WriteLine(fullMessage);
            Debug.WriteLine(fullMessage);

            IVsActivityLog log = this.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null)
            {
                return;
            }

            int hr = log.LogEntry(
                (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                this.ToString(),
                fullMessage);
        }
    }
}
