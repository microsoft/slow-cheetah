﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Represents a handler that requires user input to install/uninstall packages
    /// </summary>
    internal abstract class UserInstallationHandler : BasePackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserInstallationHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        public UserInstallationHandler(IPackageHandler successor)
            : base(successor)
        {
        }

        /// <summary>
        ///  Shows a warning message that the user must accept to continue
        /// </summary>
        /// <param name="title">The title of the message box</param>
        /// <param name="message">The message to be shown</param>
        /// <returns>True if the user has accepted the warning message</returns>
        protected async Task<bool> HasUserAcceptedWarningMessageAsync(string title, string message)
        {
            var shell = (IVsUIShell)await this.Package.GetServiceAsync(typeof(SVsUIShell));

            if (shell != null)
            {
                // Show a yes or no message box with the given title and message
                Guid compClass = Guid.Empty;
                if (ErrorHandler.Succeeded(shell.ShowMessageBox(0, ref compClass, title, message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 1, out int result)))
                {
                    return result == (int)VSConstants.MessageBoxResult.IDYES;
                }
            }

            return false;
        }
    }
}
