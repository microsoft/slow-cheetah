// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using EnvDTE;

    /// <summary>
    /// An empty handler that performs no actions
    /// </summary>
    internal class EmptyHandler : IPackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyHandler"/> class.
        /// </summary>
        /// <param name="package">VS package</param>
        public EmptyHandler(IServiceProvider package)
        {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <inheritdoc/>
        public IServiceProvider Package { get; }

        /// <inheritdoc/>
        public void Execute(Project project)
        {
            // Do nothing
        }
    }
}
