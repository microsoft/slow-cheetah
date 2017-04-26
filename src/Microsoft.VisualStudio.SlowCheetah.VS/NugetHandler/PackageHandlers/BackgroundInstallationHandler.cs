// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Performs installation operations in the background
    /// </summary>
    internal class BackgroundInstallationHandler : UserInstallationHandler
    {
        private static readonly HashSet<string> InstallTasks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object SyncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundInstallationHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public BackgroundInstallationHandler(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is an update or a new installation
        /// </summary>
        public bool IsUpdate { get; set; } = false;

        /// <inheritdoc/>
        public override void Execute(Project project)
        {
            string projName = project.UniqueName;
            bool needInstall = true;
            lock (SyncObject)
            {
                needInstall = InstallTasks.Add(projName);
            }

            if (needInstall)
            {
                string warningTitle = this.IsUpdate ? Resources.Resources.NugetUpdate_Title : Resources.Resources.NugetInstall_Title;
                string warningMessage = this.IsUpdate ? Resources.Resources.NugetUpdate_Text : Resources.Resources.NugetInstall_Text;
                if (this.HasUserAcceptedWarningMessage(warningTitle, warningMessage))
                {
                    // Gets the general output pane to inform user of installation
                    IVsOutputWindowPane outputWindow = (IVsOutputWindowPane)this.Package.GetService(typeof(SVsGeneralOutputWindowPane));
                    outputWindow?.OutputString(string.Format(Resources.Resources.NugetInstall_InstallingOutput, project.Name) + Environment.NewLine);

                    TPL.Task.Run(() =>
                    {
                        string outputMessage = Resources.Resources.NugetInstall_FinishedOutput;
                        try
                        {
                            this.Successor.Execute(project);
                        }
                        catch
                        {
                            outputMessage = Resources.Resources.NugetInstall_ErrorOutput;
                            throw;
                        }
                        finally
                        {
                            lock (SyncObject)
                            {
                                InstallTasks.Remove(projName);
                            }

                            ThreadHelper.Generic.BeginInvoke(() => outputWindow?.OutputString(string.Format(outputMessage, project.Name) + Environment.NewLine));
                        }
                    });
                }
                else
                {
                    lock (SyncObject)
                    {
                        // If the user refuses to install, the task should not be added
                        InstallTasks.Remove(projName);
                    }
                }
            }
        }
    }
}
