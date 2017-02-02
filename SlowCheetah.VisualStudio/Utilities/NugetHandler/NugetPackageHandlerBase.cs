using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;

namespace SlowCheetah.VisualStudio
{
    public abstract class NugetPackageHandlerBase : INugetPackageHandler
    {
        protected IServiceProvider package;
        protected const string pkgName = "SlowCheetah";

        protected NugetPackageHandlerBase(IServiceProvider package)
        {
            this.package = package;
        }

        public bool IsSlowCheetahInstalled(EnvDTE.Project project)
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            IVsPackageInstallerServices installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            return installerServices.IsPackageInstalled(project, pkgName);
        }

        public abstract void ShowUpdateInfo();
    }
}
