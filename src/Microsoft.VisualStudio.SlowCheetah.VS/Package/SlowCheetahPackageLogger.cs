// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Globalization;

    /// <summary>
    /// A logger class for <see cref="SlowCheetahPackage"/>
    /// </summary>
    public class SlowCheetahPackageLogger
    {
        private readonly IServiceProvider package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlowCheetahPackageLogger"/> class.
        /// </summary>
        /// <param name="package">The VSPackage</param>
        public SlowCheetahPackageLogger(IServiceProvider package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Logs a message to the Activity Log
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">The message arguments</param>
        public void LogMessage(string message, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string fullMessage = string.Format(CultureInfo.CurrentCulture, message, args);
            Trace.WriteLine(fullMessage);
            Debug.WriteLine(fullMessage);

            IVsActivityLog log = this.package.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null)
            {
                return;
            }

            int hr = log.LogEntry(
                (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION,
                this.ToString(),
                fullMessage);
        }
    }
}
