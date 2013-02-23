using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SlowCheetah.VisualStudio.Exceptions;
using SlowCheetah.VisualStudio.Properties;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Process = System.Diagnostics.Process;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet.VisualStudio;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidSlowCheetahPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideOptionPageAttribute(typeof(OptionsDialogPage), "Slow Cheetah", "General",100 ,101 ,true)]
    [ProvideProfileAttribute(typeof(OptionsDialogPage), "Slow Cheetah", "General", 100, 101, true)]
    public sealed class SlowCheetahPackage : Package
    {
        public static readonly int IDYES = 6;
        private static readonly string TransformOnBuild = "TransformOnBuild";
        private static readonly string IsTransformFile = "IsTransformFile";
        public static SlowCheetahPackage OurPackage { get; set; }
        private string pkgName = Settings.Default.SlowCheetahNugetPkgName;
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SlowCheetahPackage()
        {
            this.LogMessageWriteLineFormat("Entering constructor for: {0}", this.ToString());
            OurPackage = this;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.LogMessageWriteLineFormat("SlowCheetah initalizing");
            this.TempFilesCreated = new List<string>();

            try {
                new PackageInstaller().Install();
            }
            catch (SlowCheetahCustomException scce) {
                this.LogMessageWriteLineFormat(
                    "Unable to execute PackageInstaller.Install. [{0}] {1} [{2}]",
                    scce.ToString(),
                    Environment.NewLine,
                    scce.CustomMessage);
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("Unable to execute PackageInstaller.Install. [{0}]", ex.ToString());
                // Debug.WriteLine("Unable to execute PackageInstaller.Install. [{0}]", ex.ToString());
            }

            this.LogMessageWriteLineFormat(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs) {
                // create the command for the "Add Transform" query status menu item
                CommandID menuContextCommandID = new CommandID(GuidList.guidSlowCheetahCmdSet, (int) PkgCmdIDList.cmdidAddTransform);
                OleMenuCommand menuCommand = new OleMenuCommand(OnAddTransformCommand, OnChangeAddTransformMenu, OnBeforeQueryStatusAddTransformCommand, menuContextCommandID);
                mcs.AddCommand(menuCommand);

                // create the command for the Preview Transform menu item
                menuContextCommandID = new CommandID(GuidList.guidSlowCheetahCmdSet, (int) PkgCmdIDList.cmdidPreviewTransform);
                menuCommand = new OleMenuCommand(OnPreviewTransformCommand, OnChangePreviewTransformMenu, OnBeforeQueryStatusPreviewTransformCommand, menuContextCommandID);
                mcs.AddCommand(menuCommand);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.TempFilesCreated != null && this.TempFilesCreated.Count > 0) {
                foreach (string file in this.TempFilesCreated) {
                    try {
                        File.Delete(file);
                    }
                    catch (Exception ex) {
                        this.LogMessageWriteLineFormat(
                            "There was an error deleting a temp file [{0}], error: [{1}]",
                            file,
                            ex.Message);
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        public DTE GetDTE()
        {
            return (DTE) Package.GetGlobalService(typeof(DTE));
        }

        private IList<string> TempFilesCreated { get; set; }

        private void OnChangeAddTransformMenu(object sender, EventArgs e) { }

        private void OnChangePreviewTransformMenu(object sender, EventArgs e) { }
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
            if (menuCommand != null) {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                IVsHierarchy hierarchy = null;
                uint itemid = VSConstants.VSITEMID_NIL;

                if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) {
                    return;
                }

                IVsProject vsProject = (IVsProject) hierarchy;
                if (!ProjectSupportsTransforms(vsProject)) {
                    return;
                }

                if (!ItemSupportsTransforms(vsProject, itemid)) {
                    return;
                }

                //if (IsItemTransformItem(vsProject, itemid)) {
                //    return;
                //}

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
            if (menuCommand != null) {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                IVsHierarchy hierarchy = null;
                uint itemid = VSConstants.VSITEMID_NIL;

                if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) {
                    return;
                }

                IVsProject vsProject = (IVsProject) hierarchy;
                if (!ProjectSupportsTransforms(vsProject)) {
                    return;
                }

                //if (!ItemSupportsTransforms(vsProject, itemid)) {
                //    return;
                //}

                if (!IsItemTransformItem(vsProject, itemid)) {
                    return;
                }

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private bool IsItemTransformItem(IVsProject vsProject, uint itemid)
        {
            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null) {
                this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                return false;
            }

            bool isItemTransformFile = false;

            string value;
            buildPropertyStorage.GetItemAttribute(itemid, IsTransformFile, out value);
            if (string.Compare("true", value, true) == 0) {
                isItemTransformFile = true;
            }
            
            // we need to special case web.config transform files
            if (!isItemTransformFile) {
                string pattern = @"web\..+\.config";
                string filepath;
                buildPropertyStorage.GetItemAttribute(itemid, "FullPath", out filepath);
                if (!string.IsNullOrEmpty(filepath)) {
                    System.IO.FileInfo fi = new System.IO.FileInfo(filepath);
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(
                        pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (regex.IsMatch(fi.Name)) {
                        isItemTransformFile = true;
                    }
                }
            }

            return isItemTransformFile;
        }

        private List<string> ExcludedExtensions;
        private List<string> GetExcludedExtensions() {
            // TODO: We should be getting these values from the Tools->Options or some default list
            if (this.ExcludedExtensions == null) {
                this.ExcludedExtensions = new List<string>();
                Settings.Default.ExcludedFileExtensions.Split(';').ToList().ForEach(item => {
                    this.ExcludedExtensions.Add(item);
                });
            }

            return this.ExcludedExtensions;
        }

        /// <summary>
        /// Given a string which contains a list of extensions separated by semi-colans, this function sorts through the list
        /// throwing out any blank or empty items, and then makes sure that the extensions start with a ".". The validated list
        /// is returned.
        /// </summary>
        /// <param name="items">A list of extensions separated by semi-colans. The items may be prefixed with the ".", but don't have to be.</param>
        /// <returns>The validated and culled list of extensions. The returned items all begin with a ".".</returns>
        private string[] GetExtensionList(string items)
        {
            const string extensionPrefix = ".";

            List<string> list = new List<string>();
            if (!string.IsNullOrWhiteSpace(items)) {
                string[] extensions = items.Split(';');
                foreach (string extension in extensions) {
                    string extensionValue = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.Trim();
                    if (extensionValue != string.Empty) {
                        if (!extensionValue.StartsWith(extensionPrefix)) {
                            extensionValue = extensionPrefix + extensionValue;
                        }

                        list.Add(extensionValue);
                    }
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void OnAddTransformCommand(object sender, EventArgs e)
        {
            IVsHierarchy hierarchy = null;
            uint itemid = VSConstants.VSITEMID_NIL;

            if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) {
                return;
            }

            IVsProject vsProject = (IVsProject) hierarchy;
            if (!ProjectSupportsTransforms(vsProject)) {
                return;
            }

            string projectFullPath = null;
            if (ErrorHandler.Failed(vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectFullPath))) {
                return;
            }

            // build the import path
            string localAppData = Settings.Default.InstallUserFolderMSBuild;
            string installPath = Settings.Default.InstallPath;
            string targetsFilename = Settings.Default.TargetsFilename;
            string importPath = Path.Combine(localAppData, installPath, targetsFilename);
            
            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null) {
                this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                return;
            }

            bool addImports;
            if (!ValidateSlowCheetahTargetsAvailable(buildPropertyStorage, projectFullPath, importPath, out addImports)) {
                return;
            }

            // get the name of the item
            string itemFullPath = null;
            if (ErrorHandler.Failed(vsProject.GetMkDocument(itemid, out itemFullPath))) {
                return;
            }
            
            // Save the project file
            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            int hr = solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, hierarchy, 0);
            if(Failed(hr))
            {
                throw new COMException(string.Format(Resources.Error_SavingProjectFile, itemFullPath, GetErrorInfo()), hr);
            }
            ProjectItem selectedProjectItem = GetProjectItemFromHierarchy(hierarchy, itemid);
            Project project = null;
            if (selectedProjectItem != null) {
                // need to enure that this item has metadata TransformOnBuild set to true
                if (buildPropertyStorage != null) {
                    buildPropertyStorage.SetItemAttribute(itemid, TransformOnBuild, "true");
                }

                string itemFolder = Path.GetDirectoryName(itemFullPath);
                string itemFilename = Path.GetFileNameWithoutExtension(itemFullPath);
                string itemExtension = Path.GetExtension(itemFullPath);

                string content = BuildXdtContent(itemFullPath);
                string[] configs = GetProjectConfigurations(selectedProjectItem.ContainingProject);

                List<string> transformsToCreate = null;
                if (configs != null) { transformsToCreate = configs.ToList(); }

                if (transformsToCreate == null) { transformsToCreate = new List<string>(); }
                // if it is a web project we should add publish profile specific transforms as well
                var publishProfileTransforms = this.GetPublishProfileTransforms(hierarchy, projectFullPath);
                if (publishProfileTransforms != null) {
                    transformsToCreate.AddRange(publishProfileTransforms);
                }

                foreach (string config in transformsToCreate)
                {
                    string itemName = string.Format(Resources.String_FormatTransformFilename, itemFilename, config, itemExtension);
                    AddXdtTransformFile(selectedProjectItem, content, itemName, itemFolder);
                    uint addedFileId;
                    hierarchy.ParseCanonicalName(Path.Combine(itemFolder,itemName), out addedFileId);
                    buildPropertyStorage.SetItemAttribute(addedFileId, IsTransformFile, "True");
                }

                if (addImports) {
                    // AddSlowCheetahImport(projectFullPath, importPath);
                    if (!this.InstallSlowCheetahNuGetPackage(selectedProjectItem.ContainingProject)) {
                        // fall back for those who do not have Nuget installed
                        AddSlowCheetahImport(projectFullPath, importPath);
                    }
                }    
            }
        }


        private List<string> GetPublishProfileTransforms(IVsHierarchy hierarchy,string projectPath) {
            if (hierarchy == null) { throw new ArgumentNullException("hierarchy"); }
            if (string.IsNullOrEmpty(projectPath)) { throw new ArgumentNullException("projectPath"); }

            List<string> result = new List<string>();
            string propertiesFolder = null;
            try {
                IVsProjectSpecialFiles specialFiles = hierarchy as IVsProjectSpecialFiles;
                if (specialFiles != null) {
                    uint itemid;
                    string filePath;
                    specialFiles.GetFile((int)__PSFFILEID2.PSFFILEID_AppDesigner, (uint)__PSFFLAGS.PSFF_FullPath, out itemid, out propertiesFolder);
                }
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("Exception trying to create IVsProjectSpecialFiles", ex);
            }

            if (!string.IsNullOrEmpty(propertiesFolder)) {
                // Properties\PublishProfiles
                string publishProfilesFolder = Path.Combine(propertiesFolder, "PublishProfiles");
                if (Directory.Exists(publishProfilesFolder)) {
                    string[] publishProfiles = Directory.GetFiles(publishProfilesFolder, "*.pubxml");
                    if (publishProfiles != null) {
                        publishProfiles.ToList().ForEach(profile => {
                            FileInfo fi = new FileInfo(profile);
                            result.Add(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                        });
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void OnPreviewTransformCommand(object sender, EventArgs e)
        {
            IVsHierarchy hierarchy = null;
            uint itemId = VSConstants.VSITEMID_NIL;

            // verify only one item is selected
            if (!IsSingleProjectItemSelection(out hierarchy, out itemId)) {
                return;
            }

            // make sure that the SlowCheetah project support has been added
            IVsProject project = (IVsProject) hierarchy;
            if (!ProjectSupportsTransforms(project)) {
                // TODO: should add a dialog here telling the user that the preview failed because the targets are not yet installed
                return;
            }
            
            // get the full path of the configuration xdt
            string transformPath = null;
            if (ErrorHandler.Failed(project.GetMkDocument(itemId, out transformPath))) {
                return;
            }

            object value;
            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int) __VSHPROPID.VSHPROPID_Parent, out value));
            uint parentId = (uint) (int) value;
            if (parentId == (uint) VSConstants.VSITEMID.Nil) {
                return;
            }

            string documentPath;
            if (ErrorHandler.Failed(project.GetMkDocument(parentId, out documentPath))) {
                return;
            }

            PreviewTransform(hierarchy, documentPath, transformPath);
        }

        private void AddSlowCheetahImport(string projectFullPath, string importPath)
        {
            // save and unload the project
            DTE dte = GetDTE();

            string solutionPath = dte.Solution.FullName;
            dte.Solution.Close(true);

            // add the import
            try {
                AddSlowCheetahImportToProject(projectFullPath, importPath);
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("Exception thrown while trying to add the target import: Exception: {0}",ex);
            }

            // reload the solution
            dte.Solution.Open(solutionPath);
        }

        public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
        {
            hierarchy = null;
            itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            IVsMonitorSelection monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            IVsSolution solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null) {
                return false;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL) {
                    // there is no selection
                    return false;
                }

                if (multiItemSelect != null) {
                    // multiple items are selected
                    return false;
                }

                if (itemid == VSConstants.VSITEMID_ROOT) {
                    // there is a hierarchy root node selected, thus it is not a single item inside a project
                    return false;
                }

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null) {
                    return false;
                }

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID))) {
                    return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return true;
            }
            finally {
                if (selectionContainerPtr != IntPtr.Zero) {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero) {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }

        private bool ProjectSupportsTransforms(IVsProject project)
        {
            string projectFullPath = null;

            if (ErrorHandler.Failed(project.GetMkDocument(VSConstants.VSITEMID_ROOT, out projectFullPath))) {
                return false;
            }

            string projectExtension = Path.GetExtension(projectFullPath);

            foreach (string supportedExtension in SupportedProjectExtensions) {
                if (projectExtension.Equals(supportedExtension, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        private bool ItemSupportsTransforms(IVsProject project, uint itemid)
        {
            string itemFullPath = null;

            if (ErrorHandler.Failed(project.GetMkDocument(itemid, out itemFullPath))) {
                return false;
            }

            bool itemSupportsTransforms = false;
            // make sure its not a transform file itsle
            bool isTransformFile = IsItemTransformItem(project, itemid);

            
            FileInfo transformFileInfo = new FileInfo(itemFullPath);
            bool isWebConfig = string.Compare("web.config", transformFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0;

            if (!isWebConfig && !isTransformFile && IsExtensionSupportedForFile(itemFullPath) && IsXmlFile(itemFullPath)) {
                itemSupportsTransforms = true;
            }

            return itemSupportsTransforms;
        }

        string[] s_supportedProjectExtensions;

        

        private bool IsExtensionSupportedForFile(string filepath) {
            if (string.IsNullOrWhiteSpace(filepath)) { throw new ArgumentNullException("filepath"); }
            if (!File.Exists(filepath)) {
                throw new FileNotFoundException("File not found", filepath);
            }

            FileInfo fi = new FileInfo(filepath);
            
            var isExcludedQuery = from extension in this.GetExcludedExtensions()
                                  where string.Compare(fi.Extension, extension, StringComparison.OrdinalIgnoreCase) == 0
                                  select extension;
            var isExcluded = isExcludedQuery.Count() > 0 ? true : false;

            return !isExcluded;
        }

        private bool IsXmlFile(string filepath) {
            if (string.IsNullOrWhiteSpace(filepath)) { throw new ArgumentNullException("filepath"); }
            if (!File.Exists(filepath)) {
                throw new FileNotFoundException("File not found", filepath);
            }

            bool isXmlFile = true;
            try {
                using (XmlTextReader xmlTextReader = new XmlTextReader(filepath)) {
                    // This is required because if the XML file has a DTD then it will try and download the DTD!
                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
                    xmlTextReader.Read();
                }
            }
            catch (XmlException) {
                isXmlFile = false;
            }
            return isXmlFile;
        }

        public string[] SupportedProjectExtensions
        {
            get
            {
                if (s_supportedProjectExtensions == null) {
                    s_supportedProjectExtensions = GetSupportedExtensions(@"XdtTransforms\SupportedProjectExtensions");
                }

                return s_supportedProjectExtensions;
            }
        }

        string[] s_supportedItemExtensions;

        public string[] SupportedItemExtensions
        {
            get
            {
                if (s_supportedItemExtensions == null) {
                    s_supportedItemExtensions = GetSupportedExtensions(@"XdtTransforms\SupportedItemExtensions");
                }

                return s_supportedItemExtensions;
            }
        }

        private string[] GetSupportedExtensions(string rootKey)
        {
            IVsSettingsManager settingsManager = (IVsSettingsManager) GetService(typeof(SVsSettingsManager));
            IVsSettingsStore settings;
            ErrorHandler.ThrowOnFailure(settingsManager.GetReadOnlySettingsStore((uint) __VsSettingsScope.SettingsScope_Configuration, out settings));

            uint count = 0;
            ErrorHandler.ThrowOnFailure(settings.GetSubCollectionCount(rootKey, out count));

            string[] supportedExtensions = new string[count];

            for (uint i = 0 ; i != count ; ++i) {
                string keyName;
                ErrorHandler.ThrowOnFailure(settings.GetSubCollectionName(rootKey, i, out keyName));
                supportedExtensions[i] = keyName;
            }

            return supportedExtensions;
        }

        private void AddXdtTransformFile(ProjectItem selectedProjectItem, string content, string itemName, string projectPath)
        {
            try {
                string itemPath = Path.Combine(projectPath, itemName);
                if (!File.Exists(itemPath)) {
                    // create the new XDT file
                    using (StreamWriter writer = new StreamWriter(itemPath)) {
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
                if (buildPropertyStorage == null) {
                    this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                }
                
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("AddTransformFile: Exception> " + ex.Message);
            }
        }

        private string BuildXdtContent(string sourceItemPath)
        {
            string content = Resources.TransformContents;

            try {
                using (MemoryStream contentStream = new MemoryStream()) {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.OmitXmlDeclaration = true;
                    settings.NewLineOnAttributes = true;
                    XmlWriter contentWriter = XmlWriter.Create(contentStream, settings);

                    using (XmlReader reader = XmlReader.Create(sourceItemPath)) {
                        while (reader.Read()) {
                            if (reader.NodeType == XmlNodeType.Element) {
                                contentWriter.WriteStartElement(reader.Name, reader.NamespaceURI);
                                for (int index = 0; index < reader.AttributeCount; index++) {
                                    reader.MoveToAttribute(index);
                                    if (reader.Prefix == "xmlns" && reader.Name != "xmlns:xdt") {
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
                        using (StreamReader contentReader = new StreamReader(contentStream)) {
                            content += contentReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("Exception> " + ex.Message);
            }

            return content;
        }

        public string[] GetProjectConfigurations(EnvDTE.Project project)
        {
            List<string> configurations = new List<string>();

            if (project != null && project.ConfigurationManager != null && project.ConfigurationManager.ConfigurationRowNames != null) {
                foreach (object objConfigName in (object[]) project.ConfigurationManager.ConfigurationRowNames) {
                    string configName = objConfigName as string;
                    if (!string.IsNullOrWhiteSpace(configName)) {
                        configurations.Add(configName);
                    }
                }
            }

            return configurations.ToArray();
        }

        private bool ExistsInListIgnoreCase(string name, List<string> list)
        {
            foreach (string value in list) {
                if (string.Compare(name, value, true) == 0) {
                    return true;
                }
            }

            return false;
        }

        private ProjectItem GetProjectItemFromHierarchy(IVsHierarchy pHierarchy, uint itemID)
        {
            object propertyValue;
            ErrorHandler.ThrowOnFailure(pHierarchy.GetProperty(itemID, (int) __VSHPROPID.VSHPROPID_ExtObject, out propertyValue));
            ProjectItem projectItem = propertyValue as ProjectItem;
            if (projectItem == null) {
                this.LogMessageWriteLineFormat("ERROR: Item not found");
                return null;
            }

            return projectItem;
        }

        private void PreviewTransform(IVsHierarchy hier, string sourceFile, string transformFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile)) { throw new ArgumentNullException("sourceFile"); }
            if (string.IsNullOrWhiteSpace(transformFile)) { throw new ArgumentNullException("transformFile"); }
            if (!File.Exists(sourceFile)) { throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Error_SourceFileNotFound, sourceFile), sourceFile); }
            if (!File.Exists(transformFile)) { throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Error_TransformFileNotFound, transformFile), transformFile); }

            // Get our options
            using (OptionsDialogPage optionsPage = new OptionsDialogPage()) {
                optionsPage.LoadSettingsFromStorage();
                
                this.LogMessageWriteLineFormat("SlowCheetah PreviewTransform");
                FileInfo sourceFileInfo = new FileInfo(sourceFile);
                // dest file
                string destFile = this.GetTempFilename(true, sourceFileInfo.Extension);

                // perform the transform and then display the result into the diffmerge tool that comes with VS. If for 
                // some reason we can't find it, we just open it in an editor window
                ITransformer transformer = new Transformer();
                transformer.Transform(sourceFile, transformFile, destFile);

                // Does the customer want a preview?
                if (optionsPage.EnablePreview == false) {
                    GetDTE().ItemOperations.OpenFile(destFile);
                }
                else
                {
                    Guid SID_SVsDifferenceService = new Guid("{77115E75-EF9E-4F30-92F2-3FE78BCAF6CF}");
                    Guid IID_IVsDifferenceService = new Guid("{E20E53BE-8B7A-408F-AEA7-C0AAD6D1B946}");
                    uint VSDIFFOPT_RightFileIsTemporary = 0x00000020;   //The right file is a temporary file explicitly created for diff.

                    // If the diffmerge service is available (dev11) and no diff tool is specified, or diffmerge.exe is specifed we use the service
                    IOleServiceProvider sp;
                    hier.GetSite(out sp);
                    IntPtr diffSvcIntPtr = IntPtr.Zero;
                    int hr = sp.QueryService(ref SID_SVsDifferenceService, ref IID_IVsDifferenceService, out diffSvcIntPtr);
                    if(diffSvcIntPtr != IntPtr.Zero && (string.IsNullOrEmpty(optionsPage.PreviewToolExecutablePath) || optionsPage.PreviewToolExecutablePath.EndsWith(@"\diffmerge.exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        try {
                            object diffSvc = Marshal.GetObjectForIUnknown(diffSvcIntPtr);
                            Type t = diffSvc.GetType();
                            Type[] paramTypes = new Type[] {typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(uint)};
                            MethodInfo openComparisonWindow2 = t.GetMethod("OpenComparisonWindow2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
                            Debug.Assert(openComparisonWindow2 != null);
                            if(openComparisonWindow2 != null)
                            {
                                string sourceName = Path.GetFileName(sourceFile);
                                string leftLabel = string.Format(CultureInfo.CurrentCulture,  Resources.TransformPreview_LeftLabel, sourceName);
                                string rightLabel = string.Format(CultureInfo.CurrentCulture, Resources.TransformPreview_RightLabel, sourceName, Path.GetFileName(transformFile));
                                string caption = string.Format(CultureInfo.CurrentCulture, Resources.TransformPreview_Caption, sourceName);
                                string tooltip = string.Format(CultureInfo.CurrentCulture, Resources.TransformPreview_ToolTip, sourceName);
                                object[] paras = new object[] {sourceFile, destFile,  caption, tooltip, leftLabel, rightLabel, null, null, VSDIFFOPT_RightFileIsTemporary};
                                openComparisonWindow2.Invoke(diffSvc, paras);
                            }
                        }
                        finally {
                            Marshal.Release(diffSvcIntPtr);
                        }
                    }
                    else if (string.IsNullOrEmpty(optionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(Resources.Error_NoPreviewToolSpecified);
                    }
                    else if (!File.Exists(optionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(string.Format(Resources.Error_CantFindPreviewTool, optionsPage.PreviewToolExecutablePath), optionsPage.PreviewToolExecutablePath);
                    }
                    else
                    {
                        // Quote the filenames...
                        ProcessStartInfo psi = new ProcessStartInfo(optionsPage.PreviewToolExecutablePath, string.Format(optionsPage.PreviewToolCommandLine, "\"" + sourceFile + "\"", "\"" + destFile + "\""));
                        psi.CreateNoWindow = true;
                        psi.UseShellExecute = false;
                        Process.Start(psi);
                    }
                }
            }

            // TODO: Instead of creating a file and then deleting it later we could instead do this
            //          http://matthewmanela.com/blog/the-problem-with-the-envdte-itemoperations-newfile-method/
            //          http://social.msdn.microsoft.com/Forums/en/vsx/thread/eb032063-eb4d-42e0-84e8-dec64bf42abf
        }

        private string GetTempFilename(bool ensureFileDoesntExist, string extension = null)
        {
            string path = Path.GetTempFileName();

            if (!string.IsNullOrWhiteSpace(extension)) {
                // delete the file at path and then add the extension to it
                if (File.Exists(path)) {
                    File.Delete(path);

                    extension = extension.Trim();
                    if (!extension.StartsWith(".")) {
                        extension = "." + extension;
                    }

                    path += extension;
                }
            }

            if (ensureFileDoesntExist && File.Exists(path)) {
                File.Delete(path);
            }

            this.TempFilesCreated.Add(path);
            return path;
        }

        private bool ValidateSlowCheetahTargetsAvailable(IVsBuildPropertyStorage buildPropertyStorage, string projectFullPath, string importPath, out bool addImports)
        {
            addImports = false;

            string importsExpression = string.Format("$({0})",Settings.Default.SlowCheetahTargets);
#if USE_SLOWCHEETAH_TARGET_PROPERTY_FOR_IMPORT
            if (!IsSlowCheetahImported(buildPropertyStorage)) {
#else
            if (IsSlowCheetahImported(projectFullPath, importsExpression)) {
#endif
                return true;
            }

            if (HasUserAcceptedWarningMessage(projectFullPath, importPath)) {
                addImports = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the project file to see if the appropriate import has been made to slow Cheetah.
        /// </summary>
        /// <param name="buildPropertyStorage">Project's build property storage interface.</param>
        /// <returns>True if the SlowCheetah import is in the project file; otherwise false.</returns>
#if USE_SLOWCHEETAH_TARGET_PROPERTY_FOR_IMPORT
        private bool IsSlowCheetahImported(IVsBuildPropertyStorage buildPropertyStorage)
        {
            // check to see if the SlowCheetahImport property is set to true by the import file
            string propertyValue;
            buildPropertyStorage.GetPropertyValue(Resources.String_SlowCheetahImportProp, "|", (uint) _PersistStorageType.PST_PROJECT_FILE, out propertyValue);
            if (!string.IsNullOrWhiteSpace(propertyValue)) {
                // this property is assigned the value of $(MSBuildThisFileFullPath), which sets it to the import path of the SlowCheetah targets file
                // this is to make checking of the import fast and efficient, since this is not currently supported by VS
                return true;
            }

            return false;
        }
#else
        private bool IsSlowCheetahImported(string projectPath, string importPath)
        {
            try {
                string targetFilename = Path.GetFileName(importPath);

                ProjectRootElement projectRoot = ProjectRootElement.Open(projectPath);
                foreach (ProjectImportElement importElement in projectRoot.Imports) {
                    string importFilename = importElement.Project.Trim();
                    if (string.Compare(importFilename, targetFilename, StringComparison.OrdinalIgnoreCase) == 0) {
                        return true;
                    }
                }
            }
            catch (Exception ex) {
                this.LogMessageWriteLineFormat("Error checking to see if the SlowCheetah targets have been imported. Exception: " + ex);
            }

            return false;
        }
#endif

        /// <summary>
        /// Adds the SlowCheetah import to the given proejct item.
        /// </summary>
        /// <param name="project"></param>
        private void AddSlowCheetahImportToProject(string projectPath, string importPath)
        {
            ProjectRootElement projectRoot = ProjectRootElement.Open(projectPath);
            // TODO: This should be passed into this method
            string targetsPropertyName = Settings.Default.SlowCheetahTargets;

            var propGroup = projectRoot.AddPropertyGroup();
            ProjectPropertyElement ppe = propGroup.AddProperty(targetsPropertyName, string.Format(importPath));
            ppe.Condition = string.Format(" '$({0})'=='' ",ppe.Name);

            ProjectImportElement import = projectRoot.AddImport(string.Format("$({0})", targetsPropertyName));
            import.Condition = string.Format("Exists('$({0})')",targetsPropertyName);
            projectRoot.Save();
        }

        private bool HasUserAcceptedWarningMessage(string projectPath, string importPath)
        {
            IVsUIShell shell = GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null) {
                string message = Resources.String_AddImportText.Replace(@"\n", Environment.NewLine);
                message = string.Format(message, Path.GetFileNameWithoutExtension(projectPath), importPath);

                Guid compClass = Guid.Empty;
                int result;
                if (VSConstants.S_OK == shell.ShowMessageBox(0, ref compClass, Resources.String_AddImportTitle, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_WARNING, 1, out result)) {
                    return result == IDYES;
                }
            }

            return false;
        }

        public string GetVsInstallDirectory()
        {
            string installDirectory = null;

            IVsShell shell = GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null) {
                object installDirectoryObj = null;
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirectoryObj);
                if (installDirectoryObj != null) {
                    installDirectory = installDirectoryObj as string;
                }
            }
            return installDirectory;
        }

        private bool InstallSlowCheetahNuGetPackage(EnvDTE.Project project) {
            bool installedPackage = true;
            try {
                
                // this.LogMessageWriteLineFormat("Checking to see if the project has the
                var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
                IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                if (!installerServices.IsPackageInstalled(project, pkgName)) {
                    this.GetDTE().StatusBar.Text = "Installing SlowCheetah NuGet pacakge, this may take a few seconds";

                    IVsPackageInstaller installer = (IVsPackageInstaller)componentModel.GetService<IVsPackageInstaller>();
                    installer.InstallPackage("All", project, pkgName, (System.Version)null, false);                  

                    this.GetDTE().StatusBar.Text = "Finished installing SlowCheetah NuGet package";
                }

            }
            catch (Exception ex) {
                installedPackage = false;
                this.LogMessageWriteLineFormat("Unable to install the SlowCheetah Nuget package. {0}", ex.ToString());
            }

            return installedPackage;
        }

        public static bool Succeeded(int hr)
        {
            return(hr >= 0);
        }

        public static bool Failed(int hr)
        {
           return(hr < 0);
        }
        /// <summary>
        /// Gets the error info set on the thread. Returns empty string is none set (not null)
        /// </summary>
        public static string GetErrorInfo()
        {
            string errText = null;
            IVsUIShell uiShell = (IVsUIShell) Package.GetGlobalService(typeof(IVsUIShell));
            if(uiShell != null)
            {
                uiShell.GetErrorInfo(out errText);
            }
            if(errText == null)
                return string.Empty;
            return errText;
        }

        private void LogMessageWriteLineFormat(string message, params object[] args) {
            if (string.IsNullOrWhiteSpace(message)) { return; }

            string fullMessage = string.Format(message, args);
            Trace.WriteLine(fullMessage);
            Debug.WriteLine(fullMessage);

            IVsActivityLog log = GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null) return;

            int hr = log.LogEntry(
                (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                this.ToString(),
                fullMessage);
        }
    }
}
