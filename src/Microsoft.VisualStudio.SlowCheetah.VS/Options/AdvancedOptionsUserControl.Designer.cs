namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    partial class AdvancedOptionsUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AdvancedGroupBox = new System.Windows.Forms.GroupBox();
            this.OpenToolFileDialogButton = new System.Windows.Forms.Button();
            this.CommandLineExplanationLabel = new System.Windows.Forms.Label();
            this.PreviewToolCommandLineTextbox = new System.Windows.Forms.TextBox();
            this.PreviewCommandLineLabel = new System.Windows.Forms.Label();
            this.PreviewToolPathTextbox = new System.Windows.Forms.TextBox();
            this.PreviewToolNameLabel = new System.Windows.Forms.Label();
            this.OpenToolFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.AdvancedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // AdvancedGroupBox
            // 
            this.AdvancedGroupBox.AutoSize = true;
            this.AdvancedGroupBox.Controls.Add(this.OpenToolFileDialogButton);
            this.AdvancedGroupBox.Controls.Add(this.CommandLineExplanationLabel);
            this.AdvancedGroupBox.Controls.Add(this.PreviewToolCommandLineTextbox);
            this.AdvancedGroupBox.Controls.Add(this.PreviewCommandLineLabel);
            this.AdvancedGroupBox.Controls.Add(this.PreviewToolPathTextbox);
            this.AdvancedGroupBox.Controls.Add(this.PreviewToolNameLabel);
            this.AdvancedGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.AdvancedGroupBox.Location = new System.Drawing.Point(0, 0);
            this.AdvancedGroupBox.Name = "AdvancedGroupBox";
            this.AdvancedGroupBox.Size = new System.Drawing.Size(380, 136);
            this.AdvancedGroupBox.TabIndex = 0;
            this.AdvancedGroupBox.TabStop = false;
            this.AdvancedGroupBox.Text = "Preview Transform Tool";
            // 
            // OpenToolFileDialogButton
            // 
            this.OpenToolFileDialogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenToolFileDialogButton.AutoSize = true;
            this.OpenToolFileDialogButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.OpenToolFileDialogButton.Location = new System.Drawing.Point(348, 30);
            this.OpenToolFileDialogButton.Name = "OpenToolFileDialogButton";
            this.OpenToolFileDialogButton.Size = new System.Drawing.Size(26, 23);
            this.OpenToolFileDialogButton.TabIndex = 1;
            this.OpenToolFileDialogButton.Text = "...";
            this.OpenToolFileDialogButton.UseVisualStyleBackColor = true;
            this.OpenToolFileDialogButton.Click += new System.EventHandler(this.OpenToolFileDialogButton_Click);
            // 
            // CommandLineExplanationLabel
            // 
            this.CommandLineExplanationLabel.AutoSize = true;
            this.CommandLineExplanationLabel.Location = new System.Drawing.Point(6, 94);
            this.CommandLineExplanationLabel.Name = "CommandLineExplanationLabel";
            this.CommandLineExplanationLabel.Size = new System.Drawing.Size(242, 26);
            this.CommandLineExplanationLabel.TabIndex = 4;
            this.CommandLineExplanationLabel.Text = "The command line arguments for the preview tool.\r\n{0} is the source file, {1} is " +
    "the transformed file.";
            // 
            // PreviewToolCommandLineTextbox
            // 
            this.PreviewToolCommandLineTextbox.Location = new System.Drawing.Point(6, 71);
            this.PreviewToolCommandLineTextbox.Name = "PreviewToolCommandLineTextbox";
            this.PreviewToolCommandLineTextbox.Size = new System.Drawing.Size(239, 20);
            this.PreviewToolCommandLineTextbox.TabIndex = 3;
            this.PreviewToolCommandLineTextbox.Leave += new System.EventHandler(this.PreviewToolCommandLineTextbox_Leave);
            // 
            // PreviewCommandLineLabel
            // 
            this.PreviewCommandLineLabel.AutoSize = true;
            this.PreviewCommandLineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PreviewCommandLineLabel.Location = new System.Drawing.Point(6, 55);
            this.PreviewCommandLineLabel.Name = "PreviewCommandLineLabel";
            this.PreviewCommandLineLabel.Size = new System.Drawing.Size(148, 13);
            this.PreviewCommandLineLabel.TabIndex = 2;
            this.PreviewCommandLineLabel.Text = "Preview Tool Command Line: ";
            // 
            // PreviewToolPathTextbox
            // 
            this.PreviewToolPathTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewToolPathTextbox.Location = new System.Drawing.Point(6, 32);
            this.PreviewToolPathTextbox.Name = "PreviewToolPathTextbox";
            this.PreviewToolPathTextbox.Size = new System.Drawing.Size(336, 20);
            this.PreviewToolPathTextbox.TabIndex = 1;
            this.PreviewToolPathTextbox.Leave += new System.EventHandler(this.PreviewToolPathTextbox_Leave);
            // 
            // PreviewToolNameLabel
            // 
            this.PreviewToolNameLabel.AutoSize = true;
            this.PreviewToolNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PreviewToolNameLabel.Location = new System.Drawing.Point(6, 16);
            this.PreviewToolNameLabel.Name = "PreviewToolNameLabel";
            this.PreviewToolNameLabel.Size = new System.Drawing.Size(100, 13);
            this.PreviewToolNameLabel.TabIndex = 0;
            this.PreviewToolNameLabel.Text = "Preview Tool Path: ";
            // 
            // AdvancedOptionsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AdvancedGroupBox);
            this.Name = "AdvancedOptionsUserControl";
            this.Size = new System.Drawing.Size(380, 190);
            this.AdvancedGroupBox.ResumeLayout(false);
            this.AdvancedGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox AdvancedGroupBox;
        private System.Windows.Forms.Label PreviewToolNameLabel;
        private System.Windows.Forms.TextBox PreviewToolPathTextbox;
        private System.Windows.Forms.TextBox PreviewToolCommandLineTextbox;
        private System.Windows.Forms.Label PreviewCommandLineLabel;
        private System.Windows.Forms.Button OpenToolFileDialogButton;
        private System.Windows.Forms.Label CommandLineExplanationLabel;
        private System.Windows.Forms.OpenFileDialog OpenToolFileDialog;
    }
}
