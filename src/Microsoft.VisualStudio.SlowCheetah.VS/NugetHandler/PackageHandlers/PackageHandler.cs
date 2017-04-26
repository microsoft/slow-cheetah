﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using EnvDTE;

    /// <summary>
    /// Handles a function relating to the NuGet package
    /// </summary>
    internal abstract class PackageHandler
    {
        private PackageHandler successor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageHandler"/> class.
        /// </summary>
        /// <param name="package">VS package</param>
        protected PackageHandler(IServiceProvider package)
        {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageHandler"/> class.
        /// </summary>
        /// <param name="successor">The successor with the same package</param>
        protected PackageHandler(PackageHandler successor)
        {
            this.Successor = successor ?? throw new ArgumentNullException(nameof(successor));
            this.Package = this.Successor.Package;
        }

        /// <summary>
        /// Gets or sets the successor handler
        /// </summary>
        internal PackageHandler Successor
        {
            get
            {
                return this.successor ?? new EmptyHandler(this.Package);
            }

            set
            {
                this.successor = value;
            }
        }

        /// <summary>
        /// Gets the VS package
        /// </summary>
        protected IServiceProvider Package { get; private set; }

        /// <summary>
        /// Executes the function
        /// </summary>
        /// <param name="project">The project to act peform actions on</param>
        internal abstract void Execute(Project project);
    }
}
