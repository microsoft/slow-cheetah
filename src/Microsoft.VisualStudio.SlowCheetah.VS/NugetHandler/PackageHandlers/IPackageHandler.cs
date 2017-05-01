// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using EnvDTE;

    /// <summary>
    /// Representes a handler of nuget package actions
    /// </summary>
    internal interface IPackageHandler
    {
        /// <summary>
        /// Gets the VS package
        /// </summary>
        IServiceProvider Package { get; }

        /// <summary>
        /// Executes the function
        /// </summary>
        /// <param name="project">The project to act peform actions on</param>
        void Execute(Project project);
    }
}
