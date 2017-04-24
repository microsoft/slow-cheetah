// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah
{
    using System;

    /// <summary>
    /// Importance of a message
    /// </summary>
    public enum LogMessageImportance
    {
        /// <summary>
        /// High importace. Prioritize
        /// </summary>
        High = 2,

        /// <summary>
        /// Normal importance.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Low Importance. Do not show if unnecessary
        /// </summary>
        Low = 0
    }

    /// <summary>
    /// Interface for using an internal logger in an <see cref="ITransformer"/>
    /// </summary>
    public interface ITransformationLogger
    {
        /// <summary>
        /// Log an error
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="messageArgs">Optional message arguments</param>
        void LogError(string message, params object[] messageArgs);

        /// <summary>
        /// Log an error specifying the file, line and position
        /// </summary>
        /// <param name="file">The file containing the error</param>
        /// <param name="lineNumber">Line of the error</param>
        /// <param name="linePosition">Position of the error</param>
        /// <param name="message">The error message</param>
        /// <param name="messageArgs">Optional message arguments</param>
        void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs);

        /// <summary>
        /// Logs an error from an exception.
        /// </summary>
        /// <param name="ex">The exception</param>
        void LogErrorFromException(Exception ex);

        /// <summary>
        /// Logs an error from an exception specifying the file, line number and position
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="file">The file containing the error</param>
        /// <param name="lineNumber">Line of the error</param>
        /// <param name="linePosition">Position of the error</param>
        void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition);

        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="importance">Importance of the message</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">Optional message arguments</param>
        void LogMessage(LogMessageImportance importance, string message, params object[] messageArgs);

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="messageArgs">Optional message arguments</param>
        void LogWarning(string message, params object[] messageArgs);

        /// <summary>
        /// Log a warning specifying the file, line and position
        /// </summary>
        /// <param name="file">The file containing the warning</param>
        /// <param name="lineNumber">Line of the warning</param>
        /// <param name="linePosition">Position of the error</param>
        /// <param name="message">The warning message</param>
        /// <param name="messageArgs">Optional message arguments</param>
        void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs);
    }
}
