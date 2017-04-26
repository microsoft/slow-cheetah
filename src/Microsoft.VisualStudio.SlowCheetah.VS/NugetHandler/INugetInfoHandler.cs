// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    /// <summary>
    /// Interface for handling how to show the user information on updating SlowCheetah depending on his version of Visual Studio.
    /// Required so that incorrect assemblies are not loaded on runtime.
    /// </summary>
    public interface INugetInfoHandler
    {
        /// <summary>
        /// Shows the update info depending on what version of Visual Studio is present
        /// </summary>
        void ShowUpdateInfo();
    }
}
