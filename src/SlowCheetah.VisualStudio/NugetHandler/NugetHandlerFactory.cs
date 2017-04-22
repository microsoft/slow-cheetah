// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;

    /// <summary>
    /// Factory that determines the user's version of Visual Studio and return the correct <see cref="INugetPackageHandler"/>
    /// </summary>
    public static class NugetHandlerFactory
    {
        private static INugetPackageHandler handler;

        /// <summary>
        /// Gets the appropriate <see cref="INugetPackageHandler"/> depending on the user's Visual Stuido version
        /// </summary>
        /// <param name="package">Visual Studio Package</param>
        /// <returns>The correct handler</returns>
        public static INugetPackageHandler GetHandler(IServiceProvider package)
        {
            if (handler == null)
            {
                EnvDTE.DTE dte = ProjectUtilities.GetDTE();
                bool showInfoBar = false;
                if (Version.TryParse(dte.Version, out Version vsVersion))
                {
                    showInfoBar = vsVersion >= new Version(14, 0);
                }

                if (showInfoBar)
                {
                    handler = new NugetInfoBarHandler(package);
                }
                else
                {
                    handler = new LegacyNugetMessageHandler(package);
                }
            }

            return handler;
        }
    }
}
