extern alias Shell14;

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Shell14.Microsoft.VisualStudio.Shell;
using Shell14.Microsoft.VisualStudio.Shell.Interop;

namespace SlowCheetah.VisualStudio
{
    public class NugetInfoBarHandler : NugetPackageHandlerBase, IVsInfoBarUIEvents
    {
        private bool isInfoBarOpen;
        private uint? uiCookie;
            
        private static string UpdateLink = "UPDATE_LINK";

        public NugetInfoBarHandler(IServiceProvider package) : base(package)
        {
            isInfoBarOpen = false;
        }

        public override void ShowUpdateInfo()
        {
            var model = new InfoBarModel(
                textSpans: new[]
                {
                    new InfoBarTextSpan("It seems that you do not have the correct version of the SlowCheetah Nuget package installed. Transforms on this project will not be executed. Click "),
                    new InfoBarHyperlink("here", UpdateLink),
                    new InfoBarTextSpan(" to learn about updating from an older version or install the package.")
                }
            );
            if (TryCreateInfoBarUI(model, out var uiElement) && !isInfoBarOpen)
            {
                uiElement.Advise(this, out uint cookie);
                AddInfoBar(uiElement);
                this.uiCookie = cookie;
                this.isInfoBarOpen = true;
            }
        }

        private void AddInfoBar(IVsUIElement uiElement)
        {
            if (TryGetInfoBarHost(out var infoBarHost))
            {
                infoBarHost.AddInfoBar(uiElement);
            }
        }

        private bool TryGetInfoBarHost(out IVsInfoBarHost infoBarHost)
        {
            var shell = package.GetService(typeof(SVsShell)) as IVsShell;
            if (ErrorHandler.Failed(shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out object infoBarHostObj)))
            {
                infoBarHost = null;
                return false;
            }

            infoBarHost = infoBarHostObj as IVsInfoBarHost;
            return infoBarHost != null;
        }

        private bool TryCreateInfoBarUI(IVsInfoBar infoBar, out IVsInfoBarUIElement uiElement)
        {
            IVsInfoBarUIFactory infoBarUIFactory = package.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            if (infoBarUIFactory == null)
            {
                uiElement = null;
                return false;
            }

            uiElement = infoBarUIFactory.CreateInfoBar(infoBar);
            return uiElement != null;
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            this.isInfoBarOpen = false;
            infoBarUIElement.Unadvise(this.uiCookie.Value);
            this.uiCookie = null;
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            if (UpdateLink.Equals(actionItem.ActionContext))
            {
                System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
            }
        }
    }
}
