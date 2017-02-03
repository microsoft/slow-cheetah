// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Displays information on updating the SlowCheetah NuGet package.
    /// Opens the default web browser with the github documentation.
    /// </summary>
    public class LegacyNugetMessageHandler : INugetPackageHandler
    {
        public LegacyNugetMessageHandler(IServiceProvider package)
        {
            Package = package;
        }

        protected IServiceProvider Package { get; }

        public void ShowUpdateInfo()
        {
            System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
        }
    }
}
