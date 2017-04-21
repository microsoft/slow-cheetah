// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;

    /// <summary>
    /// Displays information on updating the SlowCheetah NuGet package.
    /// Opens the default web browser with the github documentation.
    /// </summary>
    public class LegacyNugetMessageHandler : INugetPackageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyNugetMessageHandler"/> class.
        /// </summary>
        /// <param name="package">Visual Studio Package</param>
        public LegacyNugetMessageHandler(IServiceProvider package)
        {
            this.Package = package;
        }

        /// <summary>
        /// Gets the Visual Studio Package
        /// </summary>
        protected IServiceProvider Package { get; }

        /// <inheritdoc/>
        public void ShowUpdateInfo()
        {
            System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
        }
    }
}
