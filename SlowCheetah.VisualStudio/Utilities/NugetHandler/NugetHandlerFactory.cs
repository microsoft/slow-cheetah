using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

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
                EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
                bool showInfoBar = false;
                Version vsVersion;
                System.Version.TryParse(dte.Version, out vsVersion);
                if (System.Version.TryParse(dte.Version, out vsVersion))
                {
                    showInfoBar = (vsVersion.CompareTo(new Version(14, 0)) >= 0);
                }

                if (showInfoBar)
                {
                    s_handler = new NugetInfoBarHandler(package);
                }
                else
                {
                    s_handler = new NugetMessageHandler(package);
                }
            }

            return s_handler;
        }
    }
}
