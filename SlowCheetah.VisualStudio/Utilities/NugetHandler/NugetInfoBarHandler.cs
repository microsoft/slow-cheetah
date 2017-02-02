extern alias Shell14;

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Shell14.Microsoft.VisualStudio.Shell;
using Shell14.Microsoft.VisualStudio.Shell.Interop;
using Shell14.Microsoft.VisualStudio.Imaging;

namespace SlowCheetah.VisualStudio
{
    /// <summary>
    /// Displays an info bar with update information for the SlowCheetah NuGet package
    /// </summary>
    public class NugetInfoBarHandler : INugetPackageHandler, IVsInfoBarUIEvents
    {
        private bool _isInfoBarOpen;
        private uint _uiCookie;
        protected IServiceProvider Package { get; }

        private static string UpdateLink = "SLOWCHEETAH_UPDATE_LINK";

        public NugetInfoBarHandler(IServiceProvider package)
        {
            Package = package;
            _isInfoBarOpen = false;
        }

        public void ShowUpdateInfo()
        {
            var model = new InfoBarModel(
                textSpans: new[]
                {
                    new InfoBarTextSpan(Resources.Resources.NugetUpdate_InfoBarText),
                    new InfoBarHyperlink(Resources.Resources.NugetUpdate_InfoBarLink, UpdateLink)
                },
                image: KnownMonikers.StatusInformation

            );

            IVsInfoBarUIElement uiElement;
            if (!_isInfoBarOpen && TryCreateInfoBarUI(model, out uiElement))
            {
                uint cookie;
                uiElement.Advise(this, out cookie);
                AddInfoBar(uiElement);
                _uiCookie = cookie;
                _isInfoBarOpen = true;
            }
        }

        private void AddInfoBar(IVsUIElement uiElement)
        {
            IVsInfoBarHost infoBarHost;
            if (TryGetInfoBarHost(out infoBarHost))
            {
                infoBarHost.AddInfoBar(uiElement);
            }
        }

        private bool TryGetInfoBarHost(out IVsInfoBarHost infoBarHost)
        {
            var shell = Package.GetService(typeof(SVsShell)) as IVsShell;
            object infoBarHostObj;
            if (ErrorHandler.Failed(shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out infoBarHostObj)))
            {
                infoBarHost = null;
                return false;
            }

            infoBarHost = infoBarHostObj as IVsInfoBarHost;
            return infoBarHost != null;
        }

        private bool TryCreateInfoBarUI(IVsInfoBar infoBar, out IVsInfoBarUIElement uiElement)
        {
            IVsInfoBarUIFactory infoBarUIFactory = Package.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
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
            _isInfoBarOpen = false;
            infoBarUIElement.Unadvise(_uiCookie);
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
