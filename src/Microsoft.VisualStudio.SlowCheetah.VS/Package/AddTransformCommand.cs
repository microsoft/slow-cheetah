// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Add Transform command.
    /// </summary>
    public class AddTransformCommand : BaseCommand
    {
        private readonly SlowCheetahPackage package;
        private readonly SlowCheetahNuGetManager nuGetManager;
        private readonly SlowCheetahPackageLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTransformCommand"/> class.
        /// </summary>
        /// <param name="package">The VSPackage.</param>
        /// <param name="nuGetManager">The nuget manager for the VSPackage.</param>
        /// <param name="logger">VSPackage logger.</param>
        public AddTransformCommand(SlowCheetahPackage package, SlowCheetahNuGetManager nuGetManager, SlowCheetahPackageLogger logger)
            : base(package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.nuGetManager = nuGetManager ?? throw new ArgumentNullException(nameof(nuGetManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public override int CommandId { get; } = 0x100;

        /// <inheritdoc/>
        protected override void OnChange(object sender, EventArgs e)
        {
        }

        /// <inheritdoc/>
        protected override void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
                if (!this.package.ProjectSupportsTransforms(vsProject))
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

        /// <inheritdoc/>
        protected override void OnInvoke(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            uint itemid = VSConstants.VSITEMID_NIL;

            if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemid))
            {
                return;
            }

            IVsProject vsProject = (IVsProject)hierarchy;
            if (!this.package.ProjectSupportsTransforms(vsProject))
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
                this.logger.LogMessage("Error obtaining IVsBuildPropertyStorage from hierarchy.");
                return;
            }

            // get the name of the item
            if (ErrorHandler.Failed(vsProject.GetMkDocument(itemid, out string itemFullPath)))
            {
                return;
            }

            // Save the project file
            IVsSolution solution = (IVsSolution)Shell.Package.GetGlobalService(typeof(SVsSolution));
            int hr = solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, hierarchy, 0);
            ErrorHandler.ThrowOnFailure(hr);

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
                this.package.JoinableTaskFactory.Run(() => this.nuGetManager.CheckSlowCheetahInstallationAsync(hierarchy));

                // need to enure that this item has metadata TransformOnBuild set to true
                buildPropertyStorage.SetItemAttribute(itemid, SlowCheetahPackage.TransformOnBuild, "true");

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
                        string itemName = string.Format(CultureInfo.CurrentCulture, Resources.Resources.String_FormatTransformFilename, itemFilename, config, itemExtension);
                        this.AddTransformFile(hierarchy, selectedProjectItem, itemName, itemFolder, optionsPage.AddDependentUpon);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new transformation file and adds it to the project.
        /// </summary>
        /// <param name="hierarchy">The project hierarchy.</param>
        /// <param name="selectedProjectItem">The selected item to be transformed.</param>
        /// <param name="itemName">Full name of the transformation file.</param>
        /// <param name="projectPath">Full path to the current project.</param>
        /// <param name="addDependentUpon">Wheter to add the new file dependent upon the source file.</param>
        private void AddTransformFile(
            IVsHierarchy hierarchy,
            ProjectItem selectedProjectItem,
            string itemName,
            string projectPath,
            bool addDependentUpon)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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

                // We need to set the Build Action to None to ensure that it doesn't get published for web projects
                addedItem.Properties.Item("ItemType").Value = "None";

                IVsBuildPropertyStorage buildPropertyStorage = hierarchy as IVsBuildPropertyStorage;

                if (buildPropertyStorage == null)
                {
                    this.logger.LogMessage("Error obtaining IVsBuildPropertyStorage from hierarcy.");
                }
                else if (ErrorHandler.Succeeded(hierarchy.ParseCanonicalName(addedItem.FileNames[0], out uint addedItemId)))
                {
                    buildPropertyStorage.SetItemAttribute(addedItemId, SlowCheetahPackage.IsTransformFile, "true");

                    if (addDependentUpon)
                    {
                        // Not all projects (like CPS) set the dependent upon metadata when using the automation object
                        buildPropertyStorage.GetItemAttribute(addedItemId, SlowCheetahPackage.DependentUpon, out string dependentUponValue);
                        if (string.IsNullOrEmpty(dependentUponValue))
                        {
                            // It didm not set it
                            buildPropertyStorage.SetItemAttribute(addedItemId, SlowCheetahPackage.DependentUpon, selectedProjectItem.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogMessage("AddTransformFile: Exception> " + ex.Message);
            }
        }

        /// <summary>
        /// Verifies if an item supports transforms.
        /// </summary>
        /// <param name="project">Current IVsProject.</param>
        /// <param name="itemid">Id of the item inside the project.</param>
        /// <returns>True if the item supports transforms.</returns>
        private bool ItemSupportsTransforms(IVsProject project, uint itemid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ErrorHandler.Failed(project.GetMkDocument(itemid, out string itemFullPath)))
            {
                return false;
            }

            if (!PackageUtilities.IsExtensionSupportedForFile(itemFullPath))
            {
                return false;
            }

            if (this.package.IsItemTransformItem(project, itemid))
            {
                return false;
            }

            // web.config has its own transform support
            if (string.Compare("web.config", Path.GetFileName(itemFullPath), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            // All quick checks done, ask if this is any transformer supports this.
            // This may hit the disk, which is costly for a context menu check and preferably avoided.
            return TransformerFactory.IsSupportedFile(itemFullPath);
        }

        /// <summary>
        /// Verifies any publish profiles in the project and returns it as a list of strings.
        /// </summary>
        /// <param name="hierarchy">The current project hierarchy.</param>
        /// <param name="projectPath">Full path of the current project.</param>
        /// <returns>List of publish profile names.</returns>
        private IEnumerable<string> GetPublishProfileTransforms(IVsHierarchy hierarchy, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
                this.logger.LogMessage("Exception trying to create IVsProjectSpecialFiles");
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
