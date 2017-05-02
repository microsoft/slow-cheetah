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
            this.AEnablePreviewCheckbox = new System.Windows.Forms.CheckBox();
            this.GeneralGroupBox = new System.Windows.Forms.GroupBox();
            this.BAddDepentUponCheckbox = new System.Windows.Forms.CheckBox();
            this.GeneralGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // AEnablePreviewCheckbox
            // 
            resources.ApplyResources(this.AEnablePreviewCheckbox, "AEnablePreviewCheckbox");
            this.AEnablePreviewCheckbox.Name = "AEnablePreviewCheckbox";
            this.AEnablePreviewCheckbox.UseVisualStyleBackColor = true;
            this.AEnablePreviewCheckbox.CheckedChanged += new System.EventHandler(this.EnablePreviewCheckbox_CheckedChanged);
            // 
            // GeneralGroupBox
            // 
            resources.ApplyResources(this.GeneralGroupBox, "GeneralGroupBox");
            this.GeneralGroupBox.Controls.Add(this.AEnablePreviewCheckbox);
            this.GeneralGroupBox.Controls.Add(this.BAddDepentUponCheckbox);
            this.GeneralGroupBox.Name = "GeneralGroupBox";
            this.GeneralGroupBox.TabStop = false;
            // 
            // BAddDepentUponCheckbox
            // 
            resources.ApplyResources(this.BAddDepentUponCheckbox, "BAddDepentUponCheckbox");
            this.BAddDepentUponCheckbox.Name = "BAddDepentUponCheckbox";
            this.BAddDepentUponCheckbox.UseVisualStyleBackColor = true;
            this.BAddDepentUponCheckbox.CheckedChanged += new System.EventHandler(this.AddDepentUponCheckbox_CheckedChanged);
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

        private System.Windows.Forms.CheckBox AEnablePreviewCheckbox;
        private System.Windows.Forms.GroupBox GeneralGroupBox;
        private System.Windows.Forms.CheckBox BAddDepentUponCheckbox;
    }
}
