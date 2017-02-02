using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Displays information on updating the SlowCheetah NuGet package.
    /// Opens the default web browser with the github documentation.
    /// </summary>
    public class NugetMessageHandler : INugetPackageHandler
    {
        protected IServiceProvider Package { get; }

        public NugetMessageHandler(IServiceProvider package)
        {
            Package = package;
        }

        public void ShowUpdateInfo()
        {
            System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
        }
    }
}
