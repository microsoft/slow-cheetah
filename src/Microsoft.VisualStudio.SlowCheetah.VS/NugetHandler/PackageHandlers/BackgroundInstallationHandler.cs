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
        private readonly HashSet<string> installTasks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object syncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundInstallationHandler"/> class.
        /// </summary>
        /// <param name="package">VS package</param>
        public BackgroundInstallationHandler(IServiceProvider package)
            : base(package)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is an update or a new installation
        /// </summary>
        public bool IsUpdate { get; set; } = false;

        /// <inheritdoc/>
        internal override void Execute(Project project)
        {
            string projName = project.UniqueName;
            bool needInstall = true;
            lock (this.syncObject)
            {
                needInstall = this.installTasks.Add(projName);
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
                            if (this.Successor != null)
                            {
                                this.Successor.Execute(project);
                            }
                        }
                        catch
                        {
                            outputMessage = Resources.Resources.NugetInstall_ErrorOutput;
                            throw;
                        }
                        finally
                        {
                            lock (this.syncObject)
                            {
                                this.installTasks.Remove(projName);
                            }

                            ThreadHelper.Generic.BeginInvoke(() => outputWindow?.OutputString(string.Format(outputMessage, project.Name) + Environment.NewLine));
                        }
                    });
                }
                else
                {
                    lock (this.syncObject)
                    {
                        // If the user refuses to install, the task should not be added
                        this.installTasks.Remove(projName);
                    }
                }
            }
        }
    }
}
