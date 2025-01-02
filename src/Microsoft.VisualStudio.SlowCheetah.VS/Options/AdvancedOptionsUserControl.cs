// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// The UI for the advanced section of the options page.
    /// </summary>
    public partial class AdvancedOptionsUserControl : UserControl
    {
        private AdvancedOptionsDialogPage advancedOptionsPage = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedOptionsUserControl"/> class.
        /// </summary>
        public AdvancedOptionsUserControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialized the Advanced Options Page Control.
        /// </summary>
        /// <param name="advancedOptionsPage">The options page that corresponds to this control.</param>
        internal void Initialize(AdvancedOptionsDialogPage advancedOptionsPage)
        {
            this.advancedOptionsPage = advancedOptionsPage;
            this.PreviewToolPathTextbox.Text = advancedOptionsPage.PreviewToolExecutablePath;
            this.PreviewToolCommandLineTextbox.Text = advancedOptionsPage.PreviewToolCommandLine;
        }

        private void PreviewToolPathTextbox_Leave(object sender, System.EventArgs e)
        {
            if (this.advancedOptionsPage != null)
            {
                this.advancedOptionsPage.PreviewToolExecutablePath = this.PreviewToolPathTextbox.Text;
            }
        }

        private void PreviewToolCommandLineTextbox_Leave(object sender, System.EventArgs e)
        {
            if (this.advancedOptionsPage != null)
            {
                this.advancedOptionsPage.PreviewToolCommandLine = this.PreviewToolCommandLineTextbox.Text;
            }
        }

        private void OpenToolFileDialogButton_Click(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.PreviewToolPathTextbox.Text) && File.Exists(this.PreviewToolPathTextbox.Text))
            {
                this.OpenToolFileDialog.InitialDirectory = Path.GetDirectoryName(this.PreviewToolPathTextbox.Text);
            }

            if (this.OpenToolFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.PreviewToolPathTextbox.Text = this.OpenToolFileDialog.FileName;
                if (this.advancedOptionsPage != null)
                {
                    this.advancedOptionsPage.PreviewToolExecutablePath = this.PreviewToolPathTextbox.Text;
                }
            }
        }
    }
}
