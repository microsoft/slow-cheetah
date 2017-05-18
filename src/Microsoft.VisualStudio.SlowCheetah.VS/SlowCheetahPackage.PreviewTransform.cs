// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.SlowCheetah.Exceptions;

    /// <summary>
    /// Contains logic for Preview Transform command
    /// </summary>
    public sealed partial class SlowCheetahPackage : Package, IVsUpdateSolutionEvents
    {
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
        private void OnBeforeQueryStatusPreviewTransformCommand(object sender, EventArgs e)
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
        /// <param name="sender">>The object that fired the event</param>
        /// <param name="e">Event arguments</param>
        private void OnPreviewTransformCommand(object sender, EventArgs e)
        {
            uint itemId = VSConstants.VSITEMID_NIL;

            // verify only one item is selected
            if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemId))
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
            if (ErrorHandler.Failed(project.GetMkDocument(itemId, out string transformPath)))
            {
                return;
            }

            // Checks the SlowCheetah NuGet package installation
            this.NuGetManager.CheckSlowCheetahInstallation(hierarchy);

            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Parent, out object parentIdObj));
            uint parentId = (uint)(int)parentIdObj;
            if (parentId == (uint)VSConstants.VSITEMID.Nil)
            {
                return;
            }

            if (!this.TryGetFileToTransform(hierarchy, parentId, Path.GetFileName(transformPath), out uint docId, out string documentPath))
            {
                // TO DO: Possibly tell the user that the transform file was not found.
                return;
            }

            try
            {
                PackageUtilities.GetAutomationFromHierarchy<ProjectItem>(hierarchy, docId).Save();
                PackageUtilities.GetAutomationFromHierarchy<ProjectItem>(hierarchy, itemId).Save();
            }
            catch
            {
                // If the item is not open, an exception is thrown,
                // but that is not a problem as it is not dirty
            }

            this.PreviewTransform(hierarchy, documentPath, transformPath);
        }

        /// <summary>
        /// Shows a preview of the transformation in a temporary file.
        /// </summary>
        /// <param name="hier">Current IVsHierarchy</param>
        /// <param name="sourceFile">Full path to the file to be transformed</param>
        /// <param name="transformFile">Full path to the transformation file</param>
        private void PreviewTransform(IVsHierarchy hier, string sourceFile, string transformFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (string.IsNullOrWhiteSpace(transformFile))
            {
                throw new ArgumentNullException(nameof(transformFile));
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
            using (AdvancedOptionsDialogPage advancedOptionsPage = new AdvancedOptionsDialogPage())
            {
                optionsPage.LoadSettingsFromStorage();
                advancedOptionsPage.LoadSettingsFromStorage();

                this.LogMessageWriteLineFormat("SlowCheetah PreviewTransform");
                FileInfo sourceFileInfo = new FileInfo(sourceFile);

                // dest file
                string destFile = PackageUtilities.GetTempFilename(true, sourceFileInfo.Extension);
                this.TempFilesCreated.Add(destFile);

                // perform the transform and then display the result into the diffmerge tool that comes with VS.
                // If for some reason we can't find it, we just open it in an editor window
                this.errorListProvider.Tasks.Clear();
                ITransformationLogger logger = new TransformationPreviewLogger(this.errorListProvider, hier);
                ITransformer transformer = TransformerFactory.GetTransformer(sourceFile, logger);
                if (!transformer.Transform(sourceFile, transformFile, destFile))
                {
                    throw new TransformFailedException(Resources.Resources.TransformPreview_ErrorMessage);
                }

                // Does the customer want a preview?
                if (optionsPage.EnablePreview == false)
                {
                    ProjectUtilities.GetDTE().ItemOperations.OpenFile(destFile);
                }
                else
                {
                    // If the diffmerge service is available (dev11) and no diff tool is specified, or diffmerge.exe is specifed we use the service
                    if (this.GetService(typeof(SVsDifferenceService)) is IVsDifferenceService diffService && (!File.Exists(advancedOptionsPage.PreviewToolExecutablePath) || advancedOptionsPage.PreviewToolExecutablePath.EndsWith("diffmerge.exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!string.IsNullOrEmpty(advancedOptionsPage.PreviewToolExecutablePath) && !File.Exists(advancedOptionsPage.PreviewToolExecutablePath))
                        {
                            logger.LogWarning(string.Format(Resources.Resources.Error_CantFindPreviewTool, advancedOptionsPage.PreviewToolExecutablePath));
                        }

                        string sourceName = Path.GetFileName(sourceFile);
                        string leftLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_LeftLabel, sourceName);
                        string rightLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_RightLabel, sourceName, Path.GetFileName(transformFile));
                        string caption = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_Caption, sourceName);
                        string tooltip = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_ToolTip, sourceName);
                        diffService.OpenComparisonWindow2(sourceFile, destFile, caption, tooltip, leftLabel, rightLabel, null, null, (uint)__VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary);
                    }
                    else if (string.IsNullOrEmpty(advancedOptionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(Resources.Resources.Error_NoPreviewToolSpecified);
                    }
                    else if (!File.Exists(advancedOptionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(string.Format(Resources.Resources.Error_CantFindPreviewTool, advancedOptionsPage.PreviewToolExecutablePath), advancedOptionsPage.PreviewToolExecutablePath);
                    }
                    else
                    {
                        // Quote the filenames...
                        ProcessStartInfo psi = new ProcessStartInfo(advancedOptionsPage.PreviewToolExecutablePath, string.Format(advancedOptionsPage.PreviewToolCommandLine, $"\"{sourceFile}\"", $"\"{destFile}\""))
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
        /// Searches for a file to transform based on a transformation file.
        /// Starts the search with the parent of the file then checks all visible children.
        /// </summary>
        /// <param name="hierarchy">Current project hierarchy</param>
        /// <param name="parentId">Parent ID of the file.</param>
        /// <param name="transformName">Name of the transformation file</param>
        /// <param name="docId">ID of the file to transform</param>
        /// <param name="documentPath">Resulting path of the file to transform</param>
        /// <returns>True if the correct file was found</returns>
        private bool TryGetFileToTransform(IVsHierarchy hierarchy, uint parentId, string transformName, out uint docId, out string documentPath)
        {
            IVsProject project = (IVsProject)hierarchy;

            IEnumerable<string> configs = ProjectUtilities.GetProjectConfigurations(hierarchy);

            if (ErrorHandler.Failed(project.GetMkDocument(parentId, out documentPath)))
            {
                docId = 0;
                return false;
            }

            if (PackageUtilities.IsFileTransform(Path.GetFileName(documentPath), transformName, configs))
            {
                docId = parentId;
                return true;
            }
            else
            {
                hierarchy.GetProperty(parentId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out object childIdObj);
                docId = (uint)(int)childIdObj;
                if (ErrorHandler.Failed(project.GetMkDocument(docId, out documentPath)))
                {
                    docId = 0;
                    documentPath = null;
                    return false;
                }

                if (PackageUtilities.IsFileTransform(Path.GetFileName(documentPath), transformName, configs))
                {
                    return true;
                }
                else
                {
                    while (docId != VSConstants.VSITEMID_NIL)
                    {
                        hierarchy.GetProperty(docId, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out childIdObj);
                        docId = (uint)(int)childIdObj;
                        if (ErrorHandler.Succeeded(project.GetMkDocument(docId, out documentPath)))
                        {
                            if (PackageUtilities.IsFileTransform(Path.GetFileName(documentPath), transformName, configs))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            docId = 0;
            documentPath = null;
            return false;
        }
    }
}
