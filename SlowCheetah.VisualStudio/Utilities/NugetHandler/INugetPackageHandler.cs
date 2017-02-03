// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Interface for handling how to show the user information on updating SlowCheetah depending on his version of Visual Studio.
    /// Required so that incorrect assemblies are not loaded on runtime.
    /// </summary>
    public interface INugetPackageHandler
    {
        void ShowUpdateInfo();
    }
}
