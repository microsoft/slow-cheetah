// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Base class for options page
    /// </summary>
    internal abstract class BaseOptionsDialogPage : DialogPage
    {
        /// <summary>
        /// Registry key for all options
        /// </summary>
        protected const string RegOptionsKey = "ConfigTransform";
    }
}
