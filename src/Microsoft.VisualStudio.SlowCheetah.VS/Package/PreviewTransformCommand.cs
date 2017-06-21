// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

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
    /// Preview Transform command
    /// </summary>
    public class PreviewTransformCommand : BaseCommand, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewTransformCommand"/> class.
        /// </summary>
        /// <param name="package">The VSPackage</param>
        /// <param name="nuGetManager">The nuget manager for the VSPackage</param>
        /// <param name="logger">VSPackage logger</param>
        /// <param name="errorListProvider">The VS error list provider</param>
        public PreviewTransformCommand(
            SlowCheetahPackage package,
            SlowCheetahNuGetManager nuGetManager,
            SlowCheetahPackageLogger logger,
            ErrorListProvider errorListProvider)
            : base(package)
        {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
            this.NuGetManager = nuGetManager ?? throw new ArgumentNullException(nameof(nuGetManager));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ErrorListProvider = errorListProvider ?? throw new ArgumentNullException(nameof(errorListProvider));
            this.TempFilesCreated = new List<string>();
        }

        /// <inheritdoc/>
        public override int CommandId { get; } = 0x101;

        private SlowCheetahNuGetManager NuGetManager { get; }

        private SlowCheetahPackage Package { get; }

        private SlowCheetahPackageLogger Logger { get; }

        private ErrorListProvider ErrorListProvider { get; }

        private List<string> TempFilesCreated { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (string file in this.TempFilesCreated)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    this.Logger.LogMessage(
                        "There was an error deleting a temp file [{0}], error: [{1}]",
                        file,
                        ex.Message);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnChange(object sender, EventArgs e)
        {
        }

        /// <inheritdoc/>
        protected override void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            // Get the menu that fired the event
            if (sender is OleMenuCommand menuCommand)
            {
                // Start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
                uint itemid = VSConstants.VSITEMID_NIL;

                if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemid))
                {
                    return;
                }

                IVsProject vsProject = (IVsProject)hierarchy;
                if (!this.Package.ProjectSupportsTransforms(vsProject))
                {
                    return;
                }

                // The file need to be a transform item to preview
                if (!this.Package.IsItemTransformItem(vsProject, itemid))
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
            uint itemId = VSConstants.VSITEMID_NIL;

            // Verify only one item is selected
            if (!ProjectUtilities.IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out itemId))
            {
                return;
            }

            // Make sure that the project supports transformations
            IVsProject project = (IVsProject)hierarchy;
            if (!this.Package.ProjectSupportsTransforms(project))
            {
                return;
            }

            // Get the full path of the selected file
            if (ErrorHandler.Failed(project.GetMkDocument(itemId, out string transformPath)))
            {
                return;
            }

            // Checks the SlowCheetah NuGet package installation
            this.NuGetManager.CheckSlowCheetahInstallation(hierarchy);

            // Get the parent of the file to start searching for the source file
            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Parent, out object parentIdObj));
            uint parentId = (uint)(int)parentIdObj;
            if (parentId == (uint)VSConstants.VSITEMID.Nil)
            {
                return;
            }

            if (!this.TryGetFileToTransform(hierarchy, parentId, Path.GetFileName(transformPath), out uint docId, out string documentPath))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Resources.Error_FileToTransformNotFound, transformPath));
            }

            try
            {
                // Save the source and transform files before previewing
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

                this.Logger.LogMessage("SlowCheetah PreviewTransform");
                FileInfo sourceFileInfo = new FileInfo(sourceFile);

                // Destination file
                // This should be kept as a temp file in case a custom diff tool is being used
                string destFile = PackageUtilities.GetTempFilename(true, sourceFileInfo.Extension);
                this.TempFilesCreated.Add(destFile);

                // Perform the transform and then display the result into the diffmerge tool that comes with VS.
                this.ErrorListProvider.Tasks.Clear();
                ITransformationLogger logger = new TransformationPreviewLogger(this.ErrorListProvider, hier);
                ITransformer transformer = TransformerFactory.GetTransformer(sourceFile, logger);
                if (!transformer.Transform(sourceFile, transformFile, destFile))
                {
                    throw new TransformFailedException(Resources.Resources.TransformPreview_ErrorMessage);
                }

                // Does the customer want a preview? If not, just open an editor window
                if (optionsPage.EnablePreview == false)
                {
                    ProjectUtilities.GetDTE().ItemOperations.OpenFile(destFile);
                }
                else
                {
                    // If the diffmerge service is available and no diff tool is specified, or diffmerge.exe is specifed we use the service
                    if (((IServiceProvider)this.Package).GetService(typeof(SVsDifferenceService)) is IVsDifferenceService diffService && (!File.Exists(advancedOptionsPage.PreviewToolExecutablePath) || advancedOptionsPage.PreviewToolExecutablePath.EndsWith("diffmerge.exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!string.IsNullOrEmpty(advancedOptionsPage.PreviewToolExecutablePath) && !File.Exists(advancedOptionsPage.PreviewToolExecutablePath))
                        {
                            // If the user specified a preview tool, but it doesn't exist, log a warning
                            logger.LogWarning(string.Format(CultureInfo.CurrentCulture, Resources.Resources.Error_CantFindPreviewTool, advancedOptionsPage.PreviewToolExecutablePath));
                        }

                        // Write all the labels for the diff tool
                        string sourceName = Path.GetFileName(sourceFile);
                        string leftLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_LeftLabel, sourceName);
                        string rightLabel = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_RightLabel, sourceName, Path.GetFileName(transformFile));
                        string caption = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_Caption, sourceName);
                        string tooltip = string.Format(CultureInfo.CurrentCulture, Resources.Resources.TransformPreview_ToolTip, sourceName);

                        diffService.OpenComparisonWindow2(sourceFile, destFile, caption, tooltip, leftLabel, rightLabel, null, null, (uint)__VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary);
                    }
                    else if (string.IsNullOrEmpty(advancedOptionsPage.PreviewToolExecutablePath))
                    {
                        throw new ArgumentException(Resources.Resources.Error_NoPreviewToolSpecified);
                    }
                    else if (!File.Exists(advancedOptionsPage.PreviewToolExecutablePath))
                    {
                        throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Resources.Resources.Error_CantFindPreviewTool, advancedOptionsPage.PreviewToolExecutablePath), advancedOptionsPage.PreviewToolExecutablePath);
                    }
                    else
                    {
                        // Open a process with the specified diff tool
                        // Add quotes to the file names
                        ProcessStartInfo psi = new ProcessStartInfo(advancedOptionsPage.PreviewToolExecutablePath, string.Format(CultureInfo.CurrentCulture, advancedOptionsPage.PreviewToolCommandLine, $"\"{sourceFile}\"", $"\"{destFile}\""))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
            }
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

            // Get the project configurations to use in comparing the name
            IEnumerable<string> configs = ProjectUtilities.GetProjectConfigurations(hierarchy);

            if (ErrorHandler.Failed(project.GetMkDocument(parentId, out documentPath)))
            {
                docId = 0;
                return false;
            }

            // Start by checking if the parent is the source file
            if (PackageUtilities.IsFileTransform(Path.GetFileName(documentPath), transformName, configs))
            {
                docId = parentId;
                return true;
            }
            else
            {
                // If the parent is no the file to transform, look at all of the original file's siblings
                // Starting with the parent's first visible child
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
                    // Continue on to the the next visible siblings until there are no more files to analyze
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

            // If we run out of files, the source file has not been found
            docId = 0;
            documentPath = null;
            return false;
        }
    }
}
