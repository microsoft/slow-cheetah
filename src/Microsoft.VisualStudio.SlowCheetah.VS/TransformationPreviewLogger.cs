// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable SA1512 // Single-line comments must not be followed by blank line

// Copyright (C) Sayed Ibrahim Hashimi
#pragma warning restore SA1512 // Single-line comments must not be followed by blank line

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using SlowCheetah;
    using System.Globalization;

    /// <summary>
    /// Logger for XDT transformation on Preview Transform
    /// </summary>
    public class TransformationPreviewLogger : ITransformationLogger
    {
        private readonly ErrorListProvider errorListProvider;
        private readonly IVsHierarchy hierachy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationPreviewLogger"/> class.
        /// </summary>
        /// <param name="errorListProvider">The VS Package</param>
        /// <param name="hierachy">The current project hierarchy</param>
        public TransformationPreviewLogger(ErrorListProvider errorListProvider, IVsHierarchy hierachy)
        {
            this.errorListProvider = errorListProvider ?? throw new ArgumentNullException(nameof(errorListProvider));
            this.hierachy = hierachy ?? throw new ArgumentNullException(nameof(hierachy));
        }

        /// <inheritdoc/>
        public void LogError(string message, params object[] messageArgs)
        {
            this.AddError(TaskErrorCategory.Error, string.Format(CultureInfo.CurrentCulture, message, messageArgs), null, 0, 0);
        }

        /// <inheritdoc/>
        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.AddError(TaskErrorCategory.Error, string.Format(CultureInfo.CurrentCulture, message, messageArgs), file, lineNumber, linePosition);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex)
        {
            this.AddError(ex, TaskErrorCategory.Error, null, 0, 0);
        }

        /// <inheritdoc/>
        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            this.AddError(ex, TaskErrorCategory.Error, file, lineNumber, linePosition);
        }

        /// <inheritdoc/>
        public void LogMessage(LogMessageImportance importance, string message, params object[] messageArgs)
        {
            if (importance != LogMessageImportance.Low)
            {
                this.AddError(TaskErrorCategory.Message, string.Format(CultureInfo.CurrentCulture, message, messageArgs), null, 0, 0);
            }
        }

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] messageArgs)
        {
            this.AddError(TaskErrorCategory.Warning, string.Format(CultureInfo.CurrentCulture, message, messageArgs), null, 0, 0);
        }

        /// <inheritdoc/>
        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            this.AddError(TaskErrorCategory.Warning, string.Format(CultureInfo.CurrentCulture, message, messageArgs), file, lineNumber, linePosition);
        }

        private void AddError(TaskErrorCategory errorCategory, string text, string file, int lineNumber, int linePosition)
        {
            this.ShowError(new ErrorTask(), errorCategory, text, file, lineNumber, linePosition);
        }

        private void AddError(Exception ex, TaskErrorCategory errorCategory, string file, int lineNumber, int linePosition)
        {
            this.ShowError(new ErrorTask(ex), errorCategory, ex.Message, file, lineNumber, linePosition);
        }

        private void ShowError(ErrorTask newError, TaskErrorCategory errorCategory, string text, string file, int lineNumber, int linePosition)
        {
            newError.Category = TaskCategory.Misc;
            newError.ErrorCategory = errorCategory;
            newError.Text = text;
            newError.Document = file;
            newError.Line = lineNumber;
            newError.Column = linePosition;

            newError.Navigate += (sender, e) =>
            {
                this.errorListProvider.Navigate(newError, new Guid(EnvDTE.Constants.vsViewKindCode));
            };

            this.errorListProvider.Tasks.Add(newError);
            this.errorListProvider.Show();
        }
    }
}
