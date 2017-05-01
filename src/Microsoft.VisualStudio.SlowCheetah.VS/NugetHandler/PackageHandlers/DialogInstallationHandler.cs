// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Performs installations while showing a wait dialog
    /// </summary>
    internal class DialogInstallationHandler : UserInstallationHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogInstallationHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public DialogInstallationHandler(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <inheritdoc/>
        public override void Execute(Project project)
        {
            if (this.HasUserAcceptedWarningMessage(Resources.Resources.NugetUpdate_Title, Resources.Resources.NugetUpdate_Text))
            {
                // Creates dialog informing the user to wait for the installation to finish
                IVsThreadedWaitDialogFactory twdFactory = this.Package.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                IVsThreadedWaitDialog2 dialog = null;
                twdFactory?.CreateInstance(out dialog);

                string title = Resources.Resources.NugetUpdate_WaitTitle;
                string text = Resources.Resources.NugetUpdate_WaitText;
                dialog?.StartWaitDialog(title, text, null, null, null, 0, false, true);

                try
                {
                    this.Successor.Execute(project);
                }
                finally
                {
                    // Closes the wait dialog. If the dialog failed, does nothing
                    dialog?.EndWaitDialog(out int canceled);
                }
            }
        }
    }
}
