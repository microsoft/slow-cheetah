// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

extern alias Shell14;
namespace SlowCheetah.VisualStudio
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Shell14.Microsoft.VisualStudio.Imaging;
    using Shell14.Microsoft.VisualStudio.Shell;
    using Shell14.Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Displays an info bar with update information for the SlowCheetah NuGet package
    /// </summary>
    public class NugetInfoBarHandler : INugetPackageHandler, IVsInfoBarUIEvents
    {
        private const string UpdateLink = "SLOWCHEETAH_UPDATE_LINK";

        private bool isInfoBarOpen;
        private uint uiCookie;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetInfoBarHandler"/> class.
        /// </summary>
        /// <param name="package">The Visual Studio Package</param>
        public NugetInfoBarHandler(IServiceProvider package)
        {
            this.Package = package;
            this.isInfoBarOpen = false;
        }

        /// <summary>
        /// Gets the Visual Studio Package
        /// </summary>
        protected IServiceProvider Package { get; }

        /// <inheritdoc/>
        public void ShowUpdateInfo()
        {
            var model = new InfoBarModel(
                textSpans: new[]
                {
                    new InfoBarTextSpan(Resources.Resources.NugetUpdate_InfoBarText),
                    new InfoBarHyperlink(Resources.Resources.NugetUpdate_InfoBarLink, UpdateLink)
                },
                image: KnownMonikers.StatusInformation);

            if (!this.isInfoBarOpen && this.TryCreateInfoBarUI(model, out IVsInfoBarUIElement uiElement))
            {
                uiElement.Advise(this, out uint cookie);
                this.AddInfoBar(uiElement);
                this.uiCookie = cookie;
                this.isInfoBarOpen = true;
            }
        }

        /// <inheritdoc/>
        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            this.isInfoBarOpen = false;
            infoBarUIElement.Unadvise(this.uiCookie);
        }

        /// <inheritdoc/>
        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            if (UpdateLink.Equals(actionItem.ActionContext))
            {
                System.Diagnostics.Process.Start(Resources.Resources.NugetUpdate_Link);
            }
        }

        private void AddInfoBar(IVsUIElement uiElement)
        {
            if (this.TryGetInfoBarHost(out IVsInfoBarHost infoBarHost))
            {
                infoBarHost.AddInfoBar(uiElement);
            }
        }

        private bool TryGetInfoBarHost(out IVsInfoBarHost infoBarHost)
        {
            var shell = this.Package.GetService(typeof(SVsShell)) as IVsShell;
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
            IVsInfoBarUIFactory infoBarUIFactory = this.Package.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            if (infoBarUIFactory == null)
            {
                uiElement = null;
                return false;
            }

            uiElement = infoBarUIFactory.CreateInfoBar(infoBar);
            return uiElement != null;
        }
    }
}
