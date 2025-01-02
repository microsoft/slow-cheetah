// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Threading;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Performs installation operations in the background.
    /// </summary>
    internal class BackgroundInstallationHandler : UserInstallationHandler
    {
        private static readonly HashSet<string> InstallTasks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object SyncObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundInstallationHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package.</param>
        public BackgroundInstallationHandler(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is an update or a new installation.
        /// </summary>
        public bool IsUpdate { get; set; } = false;

        /// <inheritdoc/>
        public override async TPL.Task ExecuteAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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
                if (await this.HasUserAcceptedWarningMessageAsync(warningTitle, warningMessage))
                {
                    // Gets the general output pane to inform user of installation
                    IVsOutputWindowPane outputWindow = (IVsOutputWindowPane)await this.Package.GetServiceAsync(typeof(SVsGeneralOutputWindowPane));
                    outputWindow?.OutputString(string.Format(CultureInfo.CurrentCulture, Resources.Resources.NugetInstall_InstallingOutput, project.Name) + Environment.NewLine);

                    this.Package.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TPL.TaskScheduler.Default;

                        string outputMessage = Resources.Resources.NugetInstall_FinishedOutput;
                        try
                        {
                            await this.Successor.ExecuteAsync(project);
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

                            await this.Package.JoinableTaskFactory.SwitchToMainThreadAsync();
                            outputWindow?.OutputString(string.Format(CultureInfo.CurrentCulture, outputMessage, project.Name) + Environment.NewLine);
                        }
                    }).Task.Forget();
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
