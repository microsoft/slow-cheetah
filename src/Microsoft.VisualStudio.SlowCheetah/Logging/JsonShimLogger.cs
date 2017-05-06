// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using Microsoft.VisualStudio.Jdt;

    /// <summary>
    /// Shim for using <see cref="ITransformationLogger"/>
    /// </summary>
    public class JsonShimLogger : IJsonTransformationLogger
    {
        private readonly ITransformationLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonShimLogger"/> class.
        /// </summary>
        /// <param name="logger">Our own logger</param>
        public JsonShimLogger(ITransformationLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            this.logger.LogError(message);
        }

        /// <inheritdoc/>
        public void LogError(string message, string fileName, int lineNumber, int linePosition)
        {
            this.logger.LogError(fileName, lineNumber, linePosition, message);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string fileName, int lineNumber, int linePosition)
        {
            this.logger.LogErrorFromException(ex, fileName, lineNumber, linePosition);
        }

        /// <inheritdoc/>
        public void LogMessage(string message)
        {
            this.logger.LogMessage(LogMessageImportance.Normal, message);
        }

        /// <inheritdoc/>
        public void LogMessage(string message, string fileName, int lineNumber, int linePosition)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            this.logger.LogWarning(message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName)
        {
            this.LogWarning(message, fileName, 0, 0);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, string fileName, int lineNumber, int linePosition)
        {
            this.logger.LogWarning(fileName, lineNumber, linePosition, message);
        }
    }
}
