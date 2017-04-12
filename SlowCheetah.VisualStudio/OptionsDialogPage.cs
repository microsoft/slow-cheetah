// Copyright (c) Sayed Ibrahim Hashimi. All rights reserved.
// Licensed under the Apache License, Version 2.0. See  License.md file in the project root for full license information.

namespace SlowCheetah.VisualStudio
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;

    /// <summary>
    /// Options page for SlowCheetah
    /// </summary>
    [System.Runtime.InteropServices.Guid("01B6BAC2-0BD6-4ead-95AE-6D6DE30A6286")]
    internal class OptionsDialogPage : DialogPage
    {
        private const string RegOptionsKey = "ConfigTransform";
        private const string RegPreviewCmdLine = "PreviewCmdLine";
        private const string RegPreviewExe = "PreviewExe";
        private const string RegPreviewEnable = "EnablePreview";

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsDialogPage"/> class.
        /// Constructor called by VSIP just in time when user wants to view this tools, options page
        /// </summary>
        public OptionsDialogPage()
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

        /// <summary>
        /// Gets or sets a value indicating whether preview is enabled or not
        /// </summary>
        public bool EnablePreview { get; set; }

        /// <summary>
        /// Save our settings to the specified XML writer so they can be exported
        /// </summary>
        /// <param name="writer">The VsSettings writer to write our values to</param>
        public override void SaveSettingsToXml(IVsSettingsWriter writer)
        {
            try
            {
                base.SaveSettingsToXml(writer);

                // Write settings to XML
                writer.WriteSettingString(RegPreviewExe, this.PreviewToolExecutablePath);
                writer.WriteSettingString(RegPreviewCmdLine, this.PreviewToolCommandLine);
                writer.WriteSettingBoolean(RegPreviewEnable, this.EnablePreview ? 1 : 0);
            }
            catch (Exception e)
            {
                Debug.Assert(false, "Error exporting Slow Cheetah settings: " + e.Message);
            }
        }

        /// <summary>
        /// Loads our settings to the specified XML writer
        /// </summary>
        /// <param name="reader">The VsSettings reader we read ou values from</param>
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

                if (ErrorHandler.Succeeded(reader.ReadSettingBoolean(RegPreviewEnable, out int enablePreview)))
                {
                    this.EnablePreview = enablePreview == 1;
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
                {
                    using (RegistryKey cheetahKey = userRootKey.OpenSubKey(RegOptionsKey))
                    {
                        if (cheetahKey != null)
                        {
                            object previewTool = cheetahKey.GetValue(RegPreviewExe);
                            if (previewTool != null && (previewTool is string) && !string.IsNullOrEmpty((string)previewTool))
                            {
                                this.PreviewToolExecutablePath = (string)previewTool;
                            }

                            object previewCmdLine = cheetahKey.GetValue(RegPreviewCmdLine);
                            if (previewCmdLine != null && (previewCmdLine is string) && !string.IsNullOrEmpty((string)previewCmdLine))
                            {
                                this.PreviewToolCommandLine = (string)previewCmdLine;
                            }

                            object enablePreview = cheetahKey.GetValue(RegPreviewEnable);
                            if (enablePreview != null && (enablePreview is int))
                            {
                                this.EnablePreview = ((int)enablePreview) == 1;
                            }
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
                {
                    using (RegistryKey cheetahKey = userRootKey.CreateSubKey(RegOptionsKey))
                    {
                        cheetahKey.SetValue(RegPreviewExe, this.PreviewToolExecutablePath);
                        cheetahKey.SetValue(RegPreviewCmdLine, this.PreviewToolCommandLine);
                        cheetahKey.SetValue(RegPreviewEnable, this.EnablePreview ? 1 : 0);
                    }
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
        /// <param name="e">Arguments for the event</param>
        protected override void OnDeactivate(CancelEventArgs e)
        {
        }

        /// <summary>
        /// Sets up our default values.
        /// </summary>
        private void InitializeDefaults()
        {
            string diffToolPath = SlowCheetahPackage.OurPackage.GetVsInstallDirectory();
            if (diffToolPath != null)
            {
                diffToolPath = Path.Combine(diffToolPath, "diffmerge.exe");
                this.PreviewToolExecutablePath = diffToolPath;
            }

            this.PreviewToolCommandLine = "{0} {1}";
            this.EnablePreview = true;
        }

        /// <summary>
        /// Attribute class allows us to loc the displayname for our properties. Property resource
        /// is expected to be named PropName_[propertyname]
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        internal sealed class LocDisplayNameAttribute : DisplayNameAttribute
        {
            private string displayName;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocDisplayNameAttribute"/> class.
            /// </summary>
            /// <param name="name">Attribute name</param>
            public LocDisplayNameAttribute(string name)
            {
                this.displayName = name;
            }

            /// <summary>
            /// Gets the display name of the attribute
            /// </summary>
            public override string DisplayName
            {
                get
                {
                    string result = Resources.Resources.ResourceManager.GetString("PropName_" + this.displayName);
                    if (result == null)
                    {
                        // Just return non-loc'd value
                        Debug.Assert(false, "String resource '" + this.displayName + "' is missing");
                        result = this.displayName;
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Attribute class allows us to loc the description for our properties. Property resource
        /// is expected to be named PropDesc_[propertyname]
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        internal sealed class LocDescriptionAttribute : DescriptionAttribute
        {
            private string descName;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocDescriptionAttribute"/> class.
            /// </summary>
            /// <param name="name">Attribute name</param>
            public LocDescriptionAttribute(string name)
            {
                this.descName = name;
            }

            /// <summary>
            /// Gets the description for the attribute
            /// </summary>
            public override string Description
            {
                get
                {
                    string result = Resources.Resources.ResourceManager.GetString("PropDesc_" + this.descName);

                    if (result == null)
                    {
                        // Just return non-loc'd value
                        Debug.Assert(false, "String resource '" + this.descName + "' is missing");
                        result = this.descName;
                    }

                    return result;
                }
            }
        }
    }
}
