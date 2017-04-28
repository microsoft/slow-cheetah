// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using EnvDTE;

    /// <summary>
    /// Handles a function relating to the NuGet package
    /// </summary>
    internal abstract class BasePackageHandler : IPackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasePackageHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor</param>
        protected BasePackageHandler(IPackageHandler successor)
        {
            this.Successor = successor ?? throw new ArgumentNullException(nameof(successor));
            if (successor.Package == null)
            {
                throw new ArgumentException("successor.Package must not be null");
            }

            this.Package = this.Successor.Package;
        }

        /// <summary>
        /// Gets the VS package
        /// </summary>
        public IServiceProvider Package { get; }

        /// <summary>
        /// Gets the successor handler
        /// </summary>
        protected IPackageHandler Successor { get; }

        /// <inheritdoc/>
        public abstract void Execute(Project project);
    }
}
