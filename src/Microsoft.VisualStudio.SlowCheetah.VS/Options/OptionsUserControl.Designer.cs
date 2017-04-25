namespace Microsoft.VisualStudio.SlowCheetah.VS
{
    partial class OptionsUserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsUserControl));
            this.EnablePreviewCheckbox = new System.Windows.Forms.CheckBox();
            this.GeneralGroupBox = new System.Windows.Forms.GroupBox();
            this.AddDepentUponCheckbox = new System.Windows.Forms.CheckBox();
            this.GeneralGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // EnablePreviewCheckbox
            // 
            resources.ApplyResources(this.EnablePreviewCheckbox, "EnablePreviewCheckbox");
            this.EnablePreviewCheckbox.Name = "EnablePreviewCheckbox";
            this.EnablePreviewCheckbox.UseVisualStyleBackColor = true;
            this.EnablePreviewCheckbox.CheckedChanged += new System.EventHandler(this.EnablePreviewCheckbox_CheckedChanged);
            // 
            // GeneralGroupBox
            // 
            resources.ApplyResources(this.GeneralGroupBox, "GeneralGroupBox");
            this.GeneralGroupBox.Controls.Add(this.AddDepentUponCheckbox);
            this.GeneralGroupBox.Controls.Add(this.EnablePreviewCheckbox);
            this.GeneralGroupBox.Name = "GeneralGroupBox";
            this.GeneralGroupBox.TabStop = false;
            // 
            // AddDepentUponCheckbox
            // 
            resources.ApplyResources(this.AddDepentUponCheckbox, "AddDepentUponCheckbox");
            this.AddDepentUponCheckbox.Name = "AddDepentUponCheckbox";
            this.AddDepentUponCheckbox.UseVisualStyleBackColor = true;
            this.AddDepentUponCheckbox.CheckedChanged += new System.EventHandler(this.AddDepentUponCheckbox_CheckedChanged);
            // 
            // OptionsUserControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GeneralGroupBox);
            this.Name = "OptionsUserControl";
            this.GeneralGroupBox.ResumeLayout(false);
            this.GeneralGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox EnablePreviewCheckbox;
        private System.Windows.Forms.GroupBox GeneralGroupBox;
        private System.Windows.Forms.CheckBox AddDepentUponCheckbox;
    }
}
