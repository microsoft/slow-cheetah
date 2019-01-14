// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using TPL = System.Threading.Tasks;

    /// <summary>
    /// Representes a handler of nuget package actions
    /// </summary>
    internal interface IPackageHandler
    {
        /// <summary>
        /// Gets the VS package
        /// </summary>
        AsyncPackage Package { get; }

        /// <summary>
        /// Executes the function
        /// </summary>
        /// <param name="project">The project to peform actions on</param>
        /// <returns>A task that executes the function</returns>
        TPL.Task Execute(Project project);
    }
}
