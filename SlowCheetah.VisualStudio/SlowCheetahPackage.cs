// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Xml;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using SlowCheetah.VisualStudio.Properties;

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
    [ProvideOptionPageAttribute(typeof(OptionsDialogPage), "Slow Cheetah", "General", 100, 101, true)]
    [ProvideProfileAttribute(typeof(OptionsDialogPage), "Slow Cheetah", "General", 100, 101, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class SlowCheetahPackage : Package
    {
        /// <summary>
        /// SlowCheetahPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9f2f5f20-3e0c-4c4d-9064-0351c3adec59";

        private static readonly string TransformOnBuild = "TransformOnBuild";
        private static readonly string IsTransformFile = "IsTransformFile";
        private static readonly string DependentUpon = "DependentUpon";

        private static readonly string PkgName = Settings.Default.SlowCheetahNugetPkgName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahPackage"/> class.
        /// </summary>
        public SlowCheetahPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            this.LogMessageWriteLineFormat("Entering constructor for: {0}", this.ToString());
            OurPackage = this;
        }

        /// <summary>
        /// Gets the SlowCheetahPackage
        /// </summary>
        public static SlowCheetahPackage OurPackage { get; private set; }

        private IList<string> TempFilesCreated { get; } = new List<string>();

        /// <summary>
        /// Gets the installation directory for the current instance of Visual Studio.
        /// </summary>
        /// <returns>Full path to the VS instalation directory</returns>
        public string GetVsInstallDirectory()
        {
            string installDirectory = null;
            IVsShell shell = this.GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                object installDirectoryObj;
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirectoryObj);
                if (installDirectoryObj != null)
                {
                    installDirectory = installDirectoryObj as string;
                }
            }

            return installDirectory;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.LogMessageWriteLineFormat("SlowCheetah initalizing");

            // Initialization logic
            this.LogMessageWriteLineFormat(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
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

            base.Dispose(disposing);
        }

        private void OnChangeAddTransformMenu(object sender, EventArgs e)
        {
        }

        private void OnChangePreviewTransformMenu(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// This event is fired when a user right-clicks on a menu, but prior to the menu showing. This function is used to set the visibility
        /// of the "Add Transform" menu. It checks to see if the project is one of the supported types, and if the extension of the project item
        /// that was right-clicked on is one of the valid item types.
        /// </summary>
        /// <param name="sender">The menu that fired the event.</param>
        /// <param name="e">Not used.</param>
        private void OnBeforeQueryStatusAddTransformCommand(object sender, EventArgs e)
        {
            // get the menu that fired the event
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
                uint itemid = VSConstants.VSITEMID_NIL;

                IVsHierarchy hierarchy;
                if (!ProjectUtilities.IsSingleProjectItemSelection(out hierarchy, out itemid))
                {
                    return;
                }

                IVsProject vsProject = (IVsProject)hierarchy;
                if (!this.ProjectSupportsTransforms(vsProject))
                {
                    return;
                }

                if (!this.ItemSupportsTransforms(vsProject, itemid))
                {
                    return;
                }

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        /// <summary>
        /// This event is fired when a user right-clicks on a menu, but prior to the menu showing. This function is used to set the visibility
        /// of the "Add Transform" menu. It checks to see if the project is one of the supported types, and if the extension of the project item
        /// that was right-clicked on is one of the valid item types.
        /// </summary>
        /// <param name="sender">The menu that fired the event.</param>
        /// <param name="e">Not used.</param>
        private void OnBeforeQueryStatusPreviewTransformCommand(object sender, EventArgs e)
        {
            // get the menu that fired the event
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
                uint itemid = VSConstants.VSITEMID_NIL;

                IVsHierarchy hierarchy;
                if (!ProjectUtilities.IsSingleProjectItemSelection(out hierarchy, out itemid))
                {
                    return;
                }

                IVsProject vsProject = (IVsProject)hierarchy;
                if (!this.ProjectSupportsTransforms(vsProject))
                {
                    return;
                }

                if (!this.IsItemTransformItem(vsProject, itemid))
                {
                    return;
                }

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        private void OnAddTransformCommand(object sender, EventArgs e)
        {
            uint itemid = VSConstants.VSITEMID_NIL;

            IVsHierarchy hierarchy;
            if (!ProjectUtilities.IsSingleProjectItemSelection(out hierarchy, out itemid))
            {
                return;
            }

            IVsProject vsProject = (IVsProject)hierarchy;
            if (!this.ProjectSupportsTransforms(vsProject))
            {
                return;
            }

            string projectFullPath;
            if (ErrorHandler.Failed(vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectFullPath)))
            {
                return;
            }

            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null)
            {
                this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                return;
            }

            // get the name of the item
            string itemFullPath;
            if (ErrorHandler.Failed(vsProject.GetMkDocument(itemid, out itemFullPath)))
            {
                return;
            }

            // Save the project file
            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            int hr = solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, hierarchy, 0);
            if (ErrorHandler.Failed(hr))
            {
                throw new COMException(string.Format(Resources.Resources.Error_SavingProjectFile, itemFullPath, this.GetErrorInfo()), hr);
            }

            ProjectItem selectedProjectItem = this.GetAutomationFromHierarchy<ProjectItem>(hierarchy, itemid);
            if (selectedProjectItem != null)
            {
                this.CheckSlowCheetahNugetInstallation(selectedProjectItem.ContainingProject);

                // need to enure that this item has metadata TransformOnBuild set to true
                if (buildPropertyStorage != null)
                {
                    buildPropertyStorage.SetItemAttribute(itemid, TransformOnBuild, "true");
                }

                string itemFolder = Path.GetDirectoryName(itemFullPath);
                string itemFilename = Path.GetFileNameWithoutExtension(itemFullPath);
                string itemExtension = Path.GetExtension(itemFullPath);
                string itemFilenameExtension = Path.GetFileName(itemFullPath);

                string content = this.BuildXdtContent(itemFullPath);
                IEnumerable<string> configs = ProjectUtilities.GetProjectConfigurations(selectedProjectItem.ContainingProject);

                List<string> transformsToCreate = null;
                if (configs != null)
                {
                    transformsToCreate = configs.ToList();
                }

                if (transformsToCreate == null)
                {
                    transformsToCreate = new List<string>();
                }

                // if it is a web project we should add publish profile specific transforms as well
                var publishProfileTransforms = this.GetPublishProfileTransforms(hierarchy, projectFullPath);
                if (publishProfileTransforms != null)
                {
                    transformsToCreate.AddRange(publishProfileTransforms);
                }

                foreach (string config in transformsToCreate)
                {
                    uint addedFileId;
                    string itemName = string.Format(Resources.Resources.String_FormatTransformFilename, itemFilename, config, itemExtension);
                    this.AddXdtTransformFile(selectedProjectItem, content, itemName, itemFolder);
                    hierarchy.ParseCanonicalName(Path.Combine(itemFolder, itemName), out addedFileId);
                    buildPropertyStorage.SetItemAttribute(addedFileId, IsTransformFile, "True");
                    buildPropertyStorage.SetItemAttribute(addedFileId, DependentUpon, itemFilenameExtension);
                }
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        /// <param name="sender">>The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        private void OnPreviewTransformCommand(object sender, EventArgs e)
        {
            uint itemId = VSConstants.VSITEMID_NIL;

            // verify only one item is selected
            IVsHierarchy hierarchy;
            if (!ProjectUtilities.IsSingleProjectItemSelection(out hierarchy, out itemId))
            {
                return;
            }

            // make sure that the SlowCheetah project support has been added
            IVsProject project = (IVsProject)hierarchy;
            if (!this.ProjectSupportsTransforms(project))
            {
                // TODO: should add a dialog here telling the user that the preview failed because the targets are not yet installed
                return;
            }

            // get the full path of the configuration xdt
            string transformPath;
            if (ErrorHandler.Failed(project.GetMkDocument(itemId, out transformPath)))
            {
                return;
            }

            Project currentProject = this.GetAutomationFromHierarchy<Project>(hierarchy, (uint)VSConstants.VSITEMID.Root);
            this.CheckSlowCheetahNugetInstallation(currentProject);

            object parentIdObj;
            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Parent, out parentIdObj));
            uint parentId = (uint)(int)parentIdObj;
            if (parentId == (uint)VSConstants.VSITEMID.Nil)
            {
                return;
            }

            string documentPath;
            if (ErrorHandler.Failed(project.GetMkDocument(parentId, out documentPath)))
            {
                return;
            }

            this.PreviewTransform(hierarchy, documentPath, transformPath);
        }

        private void CheckSlowCheetahNugetInstallation(Project project)
        {
            if (!this.IsSlowCheetahPackageInstalled(project))
            {
                INugetPackageHandler nugetHandler = NugetHandlerFactory.GetHandler(this);
                nugetHandler.ShowUpdateInfo();
            }
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

            string value;
            buildPropertyStorage.GetItemAttribute(itemid, IsTransformFile, out value);
            bool valueAsBool;
            if (bool.TryParse(value, out valueAsBool) && valueAsBool)
            {
                return true;
            }

            // we need to special case web.config transform files
            string pattern = @"web\..+\.config";
            string filePath;
            buildPropertyStorage.GetItemAttribute(itemid, "FullPath", out filePath);
            if (!string.IsNullOrEmpty(filePath))
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return regex.IsMatch(Path.GetFileName(filePath));
            }

            return false;
        }

        /// <summary>
        /// Verifies any publish profiles in the project and returns it as a list of strings
        /// </summary>
        /// <param name="hierarchy">The current project hierarchy</param>
        /// <param name="projectPath">Full path of the current project</param>
        /// <returns>List of publish profile names</returns>
        private IEnumerable<string> GetPublishProfileTransforms(IVsHierarchy hierarchy, string projectPath)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException("hierarchy");
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentNullException("projectPath");
            }

            List<string> result = new List<string>();
            string propertiesFolder = null;
            uint itemid;
            IVsProjectSpecialFiles specialFiles = hierarchy as IVsProjectSpecialFiles;
            if (ErrorHandler.Failed(specialFiles.GetFile((int)__PSFFILEID2.PSFFILEID_AppDesigner, (uint)__PSFFLAGS.PSFF_FullPath, out itemid, out propertiesFolder)))
            {
                this.LogMessageWriteLineFormat("Exception trying to create IVsProjectSpecialFiles");
            }

            if (!string.IsNullOrEmpty(propertiesFolder))
            {
                // Properties\PublishProfiles
                string publishProfilesFolder = Path.Combine(propertiesFolder, "PublishProfiles");
                if (Directory.Exists(publishProfilesFolder))
                {
                    string[] publishProfiles = Directory.GetFiles(publishProfilesFolder, "*.pubxml");
                    if (publishProfiles != null)
                    {
                        return publishProfiles.Select(profile => Path.GetFileNameWithoutExtension(profile));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Verifies if the current project supports transformations.
        /// </summary>
        /// <param name="project">Current IVsProject</param>
        /// <returns>True if the project supports transformation</returns>
        private bool ProjectSupportsTransforms(IVsProject project)
        {
            string projectFullPath;
            if (ErrorHandler.Failed(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectFullPath)))
            {
                return false;
            }

            string projectExtension = Path.GetExtension(projectFullPath);

            foreach (string supportedExtension in ProjectUtilities.GetSupportedProjectExtensions((IVsSettingsManager)this.GetService(typeof(SVsSettingsManager))))
            {
                if (projectExtension.Equals(supportedExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifies if an item supports transforms.
        /// </summary>
        /// <param name="project">Current IVsProject</param>
        /// <param name="itemid">Id of the item inside the project</param>
        /// <returns>True if the item supports transforms</returns>
        private bool ItemSupportsTransforms(IVsProject project, uint itemid)
        {
            string itemFullPath;
            if (ErrorHandler.Failed(project.GetMkDocument(itemid, out itemFullPath)))
            {
                return false;
            }

            bool itemSupportsTransforms = false;
            FileInfo transformFileInfo = new FileInfo(itemFullPath);

            // make sure its not a transform file itself
            bool isWebConfig = string.Compare("web.config", transformFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0;
            bool isTransformFile = this.IsItemTransformItem(project, itemid);
            bool isExtensionSupportedForFile = PackageUtilities.IsExtensionSupportedForFile(itemFullPath);
            bool isXmlFile = PackageUtilities.IsXmlFile(itemFullPath);

            if (!isWebConfig && !isTransformFile && isExtensionSupportedForFile && isXmlFile)
            {
                itemSupportsTransforms = true;
            }

            return itemSupportsTransforms;
        }

        /// <summary>
        /// Creates a new XML transformation file and adds it to the project.
        /// </summary>
        /// <param name="selectedProjectItem">The selected item to be transformed</param>
        /// <param name="content">Contents to be written to the transformation file</param>
        /// <param name="itemName">Full name of the transformation file</param>
        /// <param name="projectPath">Full path to the current project</param>
        private void AddXdtTransformFile(ProjectItem selectedProjectItem, string content, string itemName, string projectPath)
        {
            try
            {
                string itemPath = Path.Combine(projectPath, itemName);
                if (!File.Exists(itemPath))
                {
                    // create the new XDT file
                    using (StreamWriter writer = new StreamWriter(itemPath))
                    {
                        writer.Write(content);
                    }
                }

                // and add it to the project
                ProjectItem addedItem = selectedProjectItem.ProjectItems.AddFromFile(itemPath);

                // we need to set the Build Action to None to ensure that it doesn't get published for web projects
                addedItem.Properties.Item("ItemType").Value = "None";

                IVsHierarchy hierarchy = null;
                IVsProject vsProject = (IVsProject)hierarchy;
                IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
                if (buildPropertyStorage == null)
                {
                    this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                }
            }
            catch (Exception ex)
            {
                this.LogMessageWriteLineFormat("AddTransformFile: Exception> " + ex.Message);
            }
        }

        /// <summary>
        /// Builds the contents of a transformation file for a given source file.
        /// </summary>
        /// <param name="sourceItemPath">Full path to the file to be transformed.</param>
        /// <returns>Contents of the XML transform file.</returns>
        private string BuildXdtContent(string sourceItemPath)
        {
            string content = Resources.Resources.TransformContents;

            try
            {
                using (MemoryStream contentStream = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings()
                    {
                        OmitXmlDeclaration = true,
                        NewLineOnAttributes = true
                    };

                    XmlWriter contentWriter = XmlWriter.Create(contentStream, settings);

                    using (XmlReader reader = XmlReader.Create(sourceItemPath))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                contentWriter.WriteStartElement(reader.Name, reader.NamespaceURI);
                                for (int index = 0; index < reader.AttributeCount; index++)
                                {
                                    reader.MoveToAttribute(index);
                                    if (reader.Prefix == "xmlns" && reader.Name != "xmlns:xdt")
                                    {
                                        string nsName = reader.LocalName;
                                        string nsValue = reader.GetAttribute(index);
                                        contentWriter.WriteAttributeString("xmlns", nsName, null, nsValue);
                                    }
                                }

                                contentWriter.WriteAttributeString("xmlns", "xdt", null, "http://schemas.microsoft.com/XML-Document-Transform");
                                contentWriter.WriteWhitespace(Environment.NewLine);
                                contentWriter.WriteEndElement();
                                contentWriter.WriteEndDocument();
                                break;
                            }
                        }

                        contentWriter.Flush();

                        contentStream.Seek(0, SeekOrigin.Begin);
                        using (StreamReader contentReader = new StreamReader(contentStream))
                        {
                            content += contentReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogMessageWriteLineFormat("Exception> " + ex.Message);
            }

            return content;
        }

        /// <summary>
        /// Gets a ProjectItem from the hierarchy
        /// </summary>
        /// <param name="pHierarchy">Current IVsHierarchy</param>
        /// <param name="itemID">ID of the desired item in the project</param>
        /// <returns>ProjectItem corresponding to the desired item</returns>
        /// <typeparam name="T">Desired automation type to get</typeparam>
        private T GetAutomationFromHierarchy<T>(IVsHierarchy pHierarchy, uint itemID)
            where T : class
        {
            object propertyValue;
            ErrorHandler.ThrowOnFailure(pHierarchy.GetProperty(itemID, (int)__VSHPROPID.VSHPROPID_ExtObject, out propertyValue));
            T projectItem = propertyValue as T;
            if (projectItem == null)
            {
                this.LogMessageWriteLineFormat("ERROR: Item not found");
                return null;
            }

            return projectItem;
        }

        /// <summary>
        /// Shows a preview of the transformation in a temporary file.
        /// </summary>
        /// <param name="hier">Current IVsHierarchy</param>
        /// <param name="sourceFile">Full path to the file to be trasnformed</param>
        /// <param name="transformFile">Full path to the transformation file</param>
        private void PreviewTransform(IVsHierarchy hier, string sourceFile, string transformFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException("sourceFile");
            }

            if (string.IsNullOrWhiteSpace(transformFile))
            {
                throw new ArgumentNullException("transformFile");
            }

            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Resources.Error_SourceFileNotFound, sourceFile), sourceFile);
            }

            if (!File.Exists(transformFile))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Resources.Error_TransformFileNotFound, transformFile), transformFile);
            }

            // Get our options
            using (OptionsDialogPage optionsPage = new OptionsDialogPage())
            {
                optionsPage.LoadSettingsFromStorage();

                this.LogMessageWriteLineFormat("SlowCheetah PreviewTransform");
                FileInfo sourceFileInfo = new FileInfo(sourceFile);

                // dest file
                string destFile = PackageUtilities.GetTempFilename(true, sourceFileInfo.Extension);
                this.TempFilesCreated.Add(destFile);

                // perform the transform and then display the result into the diffmerge tool that comes with VS.
                // If for some reason we can't find it, we just open it in an editor window
                ITransformer transformer = new XmlTransformer();
                transformer.Transform(sourceFile, transformFile, destFile);

                // Does the customer want a preview?
                if (optionsPage.EnablePreview == false)
                {
                    ProjectUtilities.GetDTE().ItemOperations.OpenFile(destFile);
                }
                else
                {
                    // REVISE: Reference directly, not with Guid
                    Guid sID_SVsDifferenceService = new Guid("{77115E75-EF9E-4F30-92F2-3FE78BCAF6CF}");
                    Guid iID_IVsDifferenceService = new Guid("{E20E53BE-8B7A-408F-AEA7-C0AAD6D1B946}");
                    uint vSDIFFOPT_RightFileIsTemporary = 0x00000020;   // The right file is a temporary file explicitly created for diff.
                                                                        // If the diffmerge service is available (dev11) and no diff tool is specified, or diffmerge.exe is specifed we use the service
                    Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
                    hier.GetSite(out sp);
                    IntPtr diffSvcIntPtr = IntPtr.Zero;
                    int hr = sp.QueryService(ref sID_SVsDifferenceService, ref iID_IVsDifferenceService, out diffSvcIntPtr);
                    if (diffSvcIntPtr != IntPtr.Zero && (string.IsNullOrEmpty(optionsPage.PreviewToolExecutablePath) || optionsPage.PreviewToolExecutablePath.EndsWith(@"\diffmerge.exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            object diffSvc = Marshal.GetObjectForIUnknown(diffSvcIntPtr);
                            Type t = diffSvc.GetType();
                            Type[] paramTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(uint) };
                            MethodInfo openComparisonWindow2 = t.GetMethod("OpenComparisonWindow2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
                            Debug.Assert(openComparisonWindow2 != null, "The comparison window should not be null");
                            if (openComparisonWindow2 != null)
                            {
                                string sourceName = Path.GetFileName(sourceFile);
                                string leftLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_LeftLabel, sourceName);
                                string rightLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_RightLabel, sourceName, Path.GetFileName(transformFile));
                                string caption = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_Caption, sourceName);
                                string tooltip = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_ToolTip, sourceName);
                                object[] paras = new object[] { sourceFile, destFile, caption, tooltip, leftLabel, rightLabel, null, null, vSDIFFOPT_RightFileIsTemporary };
                                openComparisonWindow2.Invoke(diffSvc, paras);
                            }
                        }
                        finally
                        {
                            Marshal.Release(diffSvcIntPtr);
                        }
                    }
                    else if (string.IsNullOrEmpty(optionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(Resources.Resources.Error_NoPreviewToolSpecified);
                    }
                    else if (!File.Exists(optionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(string.Format(Resources.Resources.Error_CantFindPreviewTool, optionsPage.PreviewToolExecutablePath), optionsPage.PreviewToolExecutablePath);
                    }
                    else
                    {
                        // Quote the filenames...
                        ProcessStartInfo psi = new ProcessStartInfo(optionsPage.PreviewToolExecutablePath, string.Format(optionsPage.PreviewToolCommandLine, "\"" + sourceFile + "\"", "\"" + destFile + "\""))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
            }

            // TODO: Instead of creating a file and then deleting it later we could instead do this
            //          http://matthewmanela.com/blog/the-problem-with-the-envdte-itemoperations-newfile-method/
            //          http://social.msdn.microsoft.com/Forums/en/vsx/thread/eb032063-eb4d-42e0-84e8-dec64bf42abf
        }

        /// <summary>
        /// Verifies if the correct (updated) SlowCheetah Nuget package is installed.
        /// </summary>
        /// <param name="project">Current project</param>
        /// <returns>True if the package is installed</returns>
        private bool IsSlowCheetahPackageInstalled(EnvDTE.Project project)
        {
            var componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            if (installerServices.IsPackageInstalled(project, PkgName))
            {
                IVsPackageMetadata scPackage =
                    installerServices.GetInstalledPackages().First(pkg => string.Equals(pkg.Id, PkgName, StringComparison.OrdinalIgnoreCase));
                Version ver;
                if (Version.TryParse(scPackage.VersionString, out ver))
                {
                    return ver > new Version(2, 5, 15);
                }
            }

            return false;
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
