// Copyright (c) Sayed Ibrahim Hashimi.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.md in the project root for license information.

using System;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Factory that determines the user's version of Visual Studio and return the correct <see cref="INugetPackageHandler"/>
    /// </summary>
    public static class NugetHandlerFactory
    {
        private static INugetPackageHandler s_handler;

        public static INugetPackageHandler GetHandler(IServiceProvider package)
        {
            if (s_handler == null)
            {
                EnvDTE.DTE dte = ProjectUtilities.GetDTE();
                bool showInfoBar = false;
                Version vsVersion;
                Version.TryParse(dte.Version, out vsVersion);
                if (Version.TryParse(dte.Version, out vsVersion))
                {
                    showInfoBar = (vsVersion >= new Version(14, 0));
                }

                if (showInfoBar)
                {
                    s_handler = new NugetInfoBarHandler(package);
                }
                else
                {
                    s_handler = new LegacyNugetMessageHandler(package);
                }
            }

            return s_handler;
        }
    }
}
