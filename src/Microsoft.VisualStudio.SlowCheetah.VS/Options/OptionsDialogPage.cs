// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;

    /// <summary>
    /// Options page for SlowCheetah.
    /// </summary>
    [System.Runtime.InteropServices.Guid("01B6BAC2-0BD6-4ead-95AE-6D6DE30A6286")]
    internal class OptionsDialogPage : BaseOptionsDialogPage
    {
        private const string RegPreviewEnable = "EnablePreview";
        private const string RegDependentUpon = "EnableDependentUpon";

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsDialogPage"/> class.
        /// Constructor called by VSIP just in time when user wants to view this tools, options page.
        /// </summary>
        public OptionsDialogPage()
        {
            this.InitializeDefaults();
        }

        /// <summary>
        /// Gets or sets a value indicating whether preview is enabled or not.
        /// </summary>
        public bool EnablePreview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add DependentUpon metadata.
        /// </summary>
        public bool AddDependentUpon { get; set; }

        /// <inheritdoc/>
        protected override IWin32Window Window
        {
            get
            {
                var optionControl = new OptionsUserControl();
                optionControl.Initialize(this);
                return optionControl;
            }
        }

        /// <inheritdoc/>
        public override void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            try
            {
                base.SaveSettingsToXml(writer);

                // Write settings to XML
                writer.WriteSettingBoolean(RegPreviewEnable, this.EnablePreview ? 1 : 0);
                writer.WriteSettingBoolean(RegDependentUpon, this.AddDependentUpon ? 1 : 0);
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error exporting Slow Cheetah settings: " + e.Message);
            }
        }

        /// <inheritdoc/>
        public override void LoadSettingsFromXml(IVsSettingsReader reader)
        {
            try
            {
                this.InitializeDefaults();

                if (ErrorHandler.Succeeded(reader.ReadSettingBoolean(RegPreviewEnable, out int enablePreview)))
                {
                    this.EnablePreview = enablePreview == 1;
                }

                if (ErrorHandler.Succeeded(reader.ReadSettingBoolean(RegDependentUpon, out int addDependentUpon)))
                {
                    this.AddDependentUpon = addDependentUpon == 1;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error importing Slow Cheetah settings: " + e.Message);
            }
        }

        /// <summary>
        /// Load Tools-->Options settings from registry. Defaults are set in constructor so these
        /// will just override those values.
        /// </summary>
        public override void LoadSettingsFromStorage()
        {
            try
            {
                this.InitializeDefaults();
                using (RegistryKey userRootKey = SlowCheetahPackage.OurPackage.UserRegistryRoot)
                using (RegistryKey cheetahKey = userRootKey.OpenSubKey(RegOptionsKey))
                {
                    if (cheetahKey != null)
                    {
                        object enablePreview = cheetahKey.GetValue(RegPreviewEnable);
                        if (enablePreview != null && (enablePreview is int))
                        {
                            this.EnablePreview = ((int)enablePreview) == 1;
                        }

                        object addDependentUpon = cheetahKey.GetValue(RegDependentUpon);
                        if (addDependentUpon != null && (addDependentUpon is int))
                        {
                            this.AddDependentUpon = ((int)addDependentUpon) == 1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error reading Slow Cheetah settings from the registry: " + e.Message);
            }
        }

        /// <summary>
        /// Save Tools-->Options settings to registry.
        /// </summary>
        public override void SaveSettingsToStorage()
        {
            try
            {
                base.SaveSettingsToStorage();
                using (RegistryKey userRootKey = SlowCheetahPackage.OurPackage.UserRegistryRoot)
                using (RegistryKey cheetahKey = userRootKey.CreateSubKey(RegOptionsKey))
                {
                    cheetahKey.SetValue(RegPreviewEnable, this.EnablePreview ? 1 : 0);
                    cheetahKey.SetValue(RegDependentUpon, this.AddDependentUpon ? 1 : 0);
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error saving Slow Cheetah settings to the registry:" + e.Message);
            }
        }

        /// <summary>
        /// This event is raised when VS wants to deactivate this page.
        /// The page is deactivated unless the event is cancelled.
        /// </summary>
        /// <param name="e">Arguments for the event.</param>
        protected override void OnDeactivate(CancelEventArgs e)
        {
        }

        /// <summary>
        /// Sets up our default values.
        /// </summary>
        private void InitializeDefaults()
        {
            this.EnablePreview = true;
            this.AddDependentUpon = true;
        }
    }
}
