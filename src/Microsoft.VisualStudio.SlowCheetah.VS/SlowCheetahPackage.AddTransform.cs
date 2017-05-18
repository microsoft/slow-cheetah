// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Contains logic for Add Transform command
    /// </summary>
    public sealed partial class SlowCheetahPackage : Package, IVsUpdateSolutionEvents
    {
        private void OnChangeAddTransformMenu(object sender, EventArgs e)
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
            if (sender is OleMenuCommand menuCommand)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
                uint itemid = VSConstants.VSITEMID_NIL;

                if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemid))
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
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        private void OnAddTransformCommand(object sender, EventArgs e)
        {
            uint itemid = VSConstants.VSITEMID_NIL;

            if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemid))
            {
                return;
            }

            IVsProject vsProject = (IVsProject)hierarchy;
            if (!this.ProjectSupportsTransforms(vsProject))
            {
                return;
            }

            if (ErrorHandler.Failed(vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out string projectFullPath)))
            {
                return;
            }

            IVsBuildPropertyStorage buildPropertyStorage = vsProject as IVsBuildPropertyStorage;
            if (buildPropertyStorage == null)
            {
                this.LogMessageWriteLineFormat("Error obtaining IVsBuildPropertyStorage from hierarchy.");
                return;
            }

            // get the name of the item
            if (ErrorHandler.Failed(vsProject.GetMkDocument(itemid, out string itemFullPath)))
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

            ProjectItem selectedProjectItem = PackageUtilities.GetAutomationFromHierarchy<ProjectItem>(hierarchy, itemid);
            if (selectedProjectItem != null)
            {
                try
                {
                    selectedProjectItem.Save();
                }
                catch
                {
                    // If the item is not open, an exception is thrown,
                    // but that is not a problem as it is not dirty
                }

                // Checks the SlowCheetah NuGet package installation
                this.NuGetManager.CheckSlowCheetahInstallation(hierarchy);

                // need to enure that this item has metadata TransformOnBuild set to true
                buildPropertyStorage.SetItemAttribute(itemid, TransformOnBuild, "true");

                string itemFolder = Path.GetDirectoryName(itemFullPath);
                string itemFilename = Path.GetFileNameWithoutExtension(itemFullPath);
                string itemExtension = Path.GetExtension(itemFullPath);
                string itemFilenameExtension = Path.GetFileName(itemFullPath);

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

                using (OptionsDialogPage optionsPage = new OptionsDialogPage())
                {
                    optionsPage.LoadSettingsFromStorage();

                    foreach (string config in transformsToCreate)
                    {
                        string itemName = string.Format(Resources.Resources.String_FormatTransformFilename, itemFilename, config, itemExtension);
                        this.AddTransformFile(selectedProjectItem, itemName, itemFolder, optionsPage.AddDependentUpon);
                        hierarchy.ParseCanonicalName(Path.Combine(itemFolder, itemName), out uint addedFileId);
                        buildPropertyStorage.SetItemAttribute(addedFileId, IsTransformFile, "True");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new transformation file and adds it to the project.
        /// </summary>
        /// <param name="selectedProjectItem">The selected item to be transformed</param>
        /// <param name="itemName">Full name of the transformation file</param>
        /// <param name="projectPath">Full path to the current project</param>
        /// <param name="addDependentUpon">Wheter to add the new file dependent upon the source file</param>
        private void AddTransformFile(ProjectItem selectedProjectItem, string itemName, string projectPath, bool addDependentUpon)
        {
            try
            {
                string transformPath = Path.Combine(projectPath, itemName);
                string sourceFileName = selectedProjectItem.FileNames[1];

                ITransformer transformer = TransformerFactory.GetTransformer(sourceFileName, null);

                transformer.CreateTransformFile(sourceFileName, transformPath, false);

                // Add the file to the project
                // If the DependentUpon metadata is required, add it under the original file
                // If not, add it to the project
                ProjectItem addedItem = addDependentUpon ? selectedProjectItem.ProjectItems.AddFromFile(transformPath)
                                                      : selectedProjectItem.ContainingProject.ProjectItems.AddFromFile(transformPath);

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
        /// Verifies if an item supports transforms.
        /// </summary>
        /// <param name="project">Current IVsProject</param>
        /// <param name="itemid">Id of the item inside the project</param>
        /// <returns>True if the item supports transforms</returns>
        private bool ItemSupportsTransforms(IVsProject project, uint itemid)
        {
            if (ErrorHandler.Failed(project.GetMkDocument(itemid, out string itemFullPath)))
            {
                return false;
            }

            bool itemSupportsTransforms = false;
            FileInfo transformFileInfo = new FileInfo(itemFullPath);

            // make sure its not a transform file itself
            bool isWebConfig = string.Compare("web.config", transformFileInfo.Name, StringComparison.OrdinalIgnoreCase) == 0;
            bool isTransformFile = this.IsItemTransformItem(project, itemid);
            bool isExtensionSupportedForFile = PackageUtilities.IsExtensionSupportedForFile(itemFullPath);
            bool isSupportedFile = TransformerFactory.IsSupportedFile(itemFullPath);

            if (!isWebConfig && !isTransformFile && isExtensionSupportedForFile && isSupportedFile)
            {
                itemSupportsTransforms = true;
            }

            return itemSupportsTransforms;
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
                throw new ArgumentNullException(nameof(hierarchy));
            }

            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            List<string> result = new List<string>();
            IVsProjectSpecialFiles specialFiles = hierarchy as IVsProjectSpecialFiles;
            if (ErrorHandler.Failed(specialFiles.GetFile((int)__PSFFILEID2.PSFFILEID_AppDesigner, (uint)__PSFFLAGS.PSFF_FullPath, out uint itemid, out string propertiesFolder)))
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
    }
}
