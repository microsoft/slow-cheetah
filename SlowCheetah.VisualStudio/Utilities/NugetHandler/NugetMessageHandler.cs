using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace SlowCheetah.VisualStudio
{
    public class NugetMessageHandler : NugetPackageHandlerBase
    {
        public NugetMessageHandler(IServiceProvider package) : base(package) { }

        public override void ShowUpdateInfo()
        {
            System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
        }
    }
}
