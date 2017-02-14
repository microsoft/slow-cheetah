// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Interface for handling how to show the user information on updating SlowCheetah depending on his version of Visual Studio.
    /// Required so that incorrect assemblies are not loaded on runtime.
    /// </summary>
    public interface INugetPackageHandler
    {
        /// <summary>
        /// Shows the update info depending on what version of Visual Studio is present
        /// </summary>
        void ShowUpdateInfo();
    }
}
