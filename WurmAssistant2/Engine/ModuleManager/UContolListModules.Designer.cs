namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    partial class UControlListModules
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
            this.checkBoxUseThis = new System.Windows.Forms.CheckBox();
            this.textBoxModuleDescription = new System.Windows.Forms.TextBox();
            this.groupBoxFeatureName = new System.Windows.Forms.GroupBox();
            this.pictureBoxModuleIcon = new System.Windows.Forms.PictureBox();
            this.groupBoxFeatureName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModuleIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxUseThis
            // 
            this.checkBoxUseThis.AutoSize = true;
            this.checkBoxUseThis.Location = new System.Drawing.Point(6, 23);
            this.checkBoxUseThis.Name = "checkBoxUseThis";
            this.checkBoxUseThis.Size = new System.Drawing.Size(130, 21);
            this.checkBoxUseThis.TabIndex = 0;
            this.checkBoxUseThis.Text = "Use this feature";
            this.checkBoxUseThis.UseVisualStyleBackColor = true;
            // 
            // textBoxModuleDescription
            // 
            this.textBoxModuleDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxModuleDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxModuleDescription.Location = new System.Drawing.Point(142, 21);
            this.textBoxModuleDescription.Multiline = true;
            this.textBoxModuleDescription.Name = "textBoxModuleDescription";
            this.textBoxModuleDescription.ReadOnly = true;
            this.textBoxModuleDescription.Size = new System.Drawing.Size(475, 118);
            this.textBoxModuleDescription.TabIndex = 1;
            this.textBoxModuleDescription.TabStop = false;
            // 
            // groupBoxFeatureName
            // 
            this.groupBoxFeatureName.Controls.Add(this.pictureBoxModuleIcon);
            this.groupBoxFeatureName.Controls.Add(this.textBoxModuleDescription);
            this.groupBoxFeatureName.Controls.Add(this.checkBoxUseThis);
            this.groupBoxFeatureName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFeatureName.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFeatureName.Name = "groupBoxFeatureName";
            this.groupBoxFeatureName.Size = new System.Drawing.Size(623, 145);
            this.groupBoxFeatureName.TabIndex = 2;
            this.groupBoxFeatureName.TabStop = false;
            this.groupBoxFeatureName.Text = "FeatureName";
            // 
            // pictureBoxModuleIcon
            // 
            this.pictureBoxModuleIcon.Location = new System.Drawing.Point(28, 50);
            this.pictureBoxModuleIcon.Name = "pictureBoxModuleIcon";
            this.pictureBoxModuleIcon.Size = new System.Drawing.Size(80, 80);
            this.pictureBoxModuleIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxModuleIcon.TabIndex = 2;
            this.pictureBoxModuleIcon.TabStop = false;
            // 
            // UControlListModules
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxFeatureName);
            this.Name = "UControlListModules";
            this.Size = new System.Drawing.Size(623, 145);
            this.groupBoxFeatureName.ResumeLayout(false);
            this.groupBoxFeatureName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModuleIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxUseThis;
        private System.Windows.Forms.TextBox textBoxModuleDescription;
        private System.Windows.Forms.GroupBox groupBoxFeatureName;
        private System.Windows.Forms.PictureBox pictureBoxModuleIcon;
    }
}
