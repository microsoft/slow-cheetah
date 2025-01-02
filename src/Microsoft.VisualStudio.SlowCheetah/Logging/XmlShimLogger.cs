// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Web.XmlTransform;

    /// <summary>
    /// Shim for using an <see cref="ITransformationLogger"/> as a <see cref="IXmlTransformationLogger"/>.
    /// </summary>
    public class XmlShimLogger : IXmlTransformationLogger
    {
        private static readonly string IndentStringPiece = "  ";

        private readonly bool useSections;

        private readonly ITransformationLogger logger;

        private int indentLevel = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlShimLogger"/> class.
        /// </summary>
        /// <param name="logger">Our own logger.</param>
        /// <param name="useSections">Wheter or not to use sections.</param>
        public XmlShimLogger(ITransformationLogger logger, bool useSections)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.useSections = useSections;
        }

        private string IndentString
        {
            get
            {
                if (this.indentLevel == 0)
                {
                    return string.Empty;
                }

                return string.Concat(Enumerable.Repeat(IndentStringPiece, this.indentLevel));
            }
        }

        /// <inheritdoc/>
        public void LogError(string message, params object[] messageArgs)
        {
            this.logger.LogError(message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogError(string file, string message, params object[] messageArgs)
        {
            this.logger.LogError(file, 0, 0, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.logger.LogError(file, lineNumber, linePosition, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string file)
        {
            this.logger.LogErrorFromException(ex, file, 0, 0);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            this.logger.LogErrorFromException(ex, file, lineNumber, linePosition);
        }

        /// <inheritdoc/>
        public void LogMessage(string message, params object[] messageArgs)
        {
            if (this.useSections)
            {
                message = string.Concat(this.IndentString, message);
            }

            this.logger.LogMessage(LogMessageImportance.Normal, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            LogMessageImportance importance;
            switch (type)
            {
                case MessageType.Normal:
                    importance = LogMessageImportance.Normal;
                    break;
                case MessageType.Verbose:
                    importance = LogMessageImportance.Low;
                    break;
                default:
                    Debug.Fail("Unknown MessageType");
                    importance = LogMessageImportance.Normal;
                    break;
            }

            if (this.useSections)
            {
                message = string.Concat(this.IndentString, message);
            }

            this.logger.LogMessage(importance, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] messageArgs)
        {
            this.logger.LogWarning(message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            this.logger.LogWarning(file, 0, 0, message, messageArgs);
        }

        /// <inheritdoc/>
        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.logger.LogWarning(file, lineNumber, linePosition, message, messageArgs);
        }

        /// <inheritdoc/>
        public void StartSection(string message, params object[] messageArgs)
        {
            this.StartSection(MessageType.Normal, message, messageArgs);
        }

        /// <inheritdoc/>
        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.useSections)
            {
                this.LogMessage(message, messageArgs);
                this.indentLevel++;
            }
        }

        /// <inheritdoc/>
        public void EndSection(string message, params object[] messageArgs)
        {
            this.EndSection(MessageType.Normal, message, messageArgs);
        }

        /// <inheritdoc/>
        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.useSections)
            {
                Debug.Assert(this.indentLevel > 0, "There must be at least one section started");
                if (this.indentLevel > 0)
                {
                    this.indentLevel--;
                }

                this.LogMessage(type, message, messageArgs);
            }
        }
    }
}
