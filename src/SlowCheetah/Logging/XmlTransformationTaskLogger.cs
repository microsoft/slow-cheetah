// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace SlowCheetah
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.Web.XmlTransform;

    /// <summary>
    /// Shim for using MSBuild logger in <see cref="XmlTransformation"/>
    /// </summary>
    public class XmlTransformationTaskLogger : ITransformationLogger
    {
        private readonly TaskLoggingHelper loggingHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTransformationTaskLogger"/> class.
        /// </summary>
        /// <param name="logger">The MSBuild logger</param>
        public XmlTransformationTaskLogger(TaskLoggingHelper logger)
        {
            this.loggingHelper = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void LogError(string message, params object[] messageArgs)
        {
            this.loggingHelper.LogError(message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.loggingHelper.LogError(null, null, null, file, lineNumber, linePosition, 0, 0, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.loggingHelper.LogErrorFromException(ex);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            this.LogError(file, lineNumber, linePosition, ex.Message);
        }

        /// <inheritdoc/>
        public void LogMessage(LogMessageImportance type, string message, params object[] messageArgs)
        {
            MessageImportance importance;
            switch (type)
            {
                case LogMessageImportance.High:
                    importance = MessageImportance.High;
                    break;
                case LogMessageImportance.Normal:
                    importance = MessageImportance.Normal;
                    break;
                case LogMessageImportance.Low:
                    importance = MessageImportance.Low;
                    break;
                default:
                    Debug.Fail("Unknown LogMessageImportance");
                    importance = MessageImportance.Normal;
                    break;
            }

            this.loggingHelper.LogMessage(importance, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] messageArgs)
        {
            this.loggingHelper.LogWarning(message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.loggingHelper.LogWarning(null, null, null, file, lineNumber, linePosition, 0, 0, message, messageArgs);
        }
    }
}
