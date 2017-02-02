using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SlowCheetah.VisualStudio
{
    public static class NugetHandlerFactory
    {
        private static INugetPackageHandler s_ourHandler;

        public static INugetPackageHandler GetHandler(IServiceProvider package)
        {
            if (s_ourHandler == null)
            {
                EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
                bool showInfoBar = false;
                if (Int32.TryParse(dte.Version.Split('.').First(), out int vNumber))
                {
                    showInfoBar = vNumber >= 14;
                }

                if (showInfoBar)
                {
                    s_ourHandler = new NugetInfoBarHandler(package);
                }
                else
                {
                    s_ourHandler = new NugetMessageHandler(package);
                }
            }

            return s_ourHandler;
        }
    }
}
