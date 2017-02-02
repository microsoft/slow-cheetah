using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Interface for handling how to show the user information on updating SlowCheetah depending on his version of Visual Studio.
    /// Required so that incorrect assemblies are not loaded on runtime.
    /// </summary>
    public interface INugetPackageHandler
    {
        void ShowUpdateInfo();
    }
}
