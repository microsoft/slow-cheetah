// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;

    /// <summary>
    /// Advanced Options Page for SlowCheetah
    /// </summary>
    internal class AdvancedOptionsDialogPage : BaseOptionsDialogPage
    {
        private const string RegPreviewCmdLine = "PreviewCmdLine";
        private const string RegPreviewExe = "PreviewExe";

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedOptionsDialogPage"/> class.
        /// Constructor called by VSIP just in time when user wants to view this tools, options page
        /// </summary>
        public AdvancedOptionsDialogPage()
        {
            this.InitializeDefaults();
        }

        /// <summary>
        /// Gets or sets the exe path for the diff tool used to preview transformations
        /// </summary>
        public string PreviewToolExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets the required command on execution of the preview tool
        /// </summary>
        public string PreviewToolCommandLine { get; set; }

        /// <inheritdoc/>
        protected override IWin32Window Window
        {
            get
            {
                var optionControl = new AdvancedOptionsUserControl();
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
                writer.WriteSettingString(RegPreviewExe, this.PreviewToolExecutablePath);
                writer.WriteSettingString(RegPreviewCmdLine, this.PreviewToolCommandLine);
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
                if (ErrorHandler.Succeeded(reader.ReadSettingString(RegPreviewExe, out string exePath)) && !string.IsNullOrEmpty(exePath))
                {
                    this.PreviewToolExecutablePath = exePath;
                }

                if (ErrorHandler.Succeeded(reader.ReadSettingString(RegPreviewCmdLine, out string exeCmdLine)) && !string.IsNullOrEmpty(exeCmdLine))
                {
                    this.PreviewToolCommandLine = exeCmdLine;
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
                        object previewTool = cheetahKey.GetValue(RegPreviewExe);
                        if (previewTool != null && (previewTool is string))
                        {
                            this.PreviewToolExecutablePath = (string)previewTool;
                        }

                        object previewCmdLine = cheetahKey.GetValue(RegPreviewCmdLine);
                        if (previewCmdLine != null && (previewCmdLine is string))
                        {
                            this.PreviewToolCommandLine = (string)previewCmdLine;
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
        /// Save Tools-->Options settings to registry
        /// </summary>
        public override void SaveSettingsToStorage()
        {
            try
            {
                base.SaveSettingsToStorage();
                using (RegistryKey userRootKey = SlowCheetahPackage.OurPackage.UserRegistryRoot)
                using (RegistryKey cheetahKey = userRootKey.CreateSubKey(RegOptionsKey))
                {
                    cheetahKey.SetValue(RegPreviewExe, this.PreviewToolExecutablePath);
                    cheetahKey.SetValue(RegPreviewCmdLine, this.PreviewToolCommandLine);
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error saving Slow Cheetah settings to the registry:" + e.Message);
            }
        }

        /// <summary>
        /// Sets up our default values.
        /// </summary>
        private void InitializeDefaults()
        {
            this.PreviewToolExecutablePath = string.Empty;
            this.PreviewToolCommandLine = "{0} {1}";
        }
    }
}
