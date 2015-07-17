namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    partial class FormTriggersMain
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTriggersMain));
            this.buttonManageSounds = new System.Windows.Forms.Button();
            this.trackBarGlobalVolume = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonAddNew = new System.Windows.Forms.Button();
            this.buttonMute = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGlobalVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonManageSounds
            // 
            this.buttonManageSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonManageSounds.Location = new System.Drawing.Point(272, 10);
            this.buttonManageSounds.Margin = new System.Windows.Forms.Padding(2);
            this.buttonManageSounds.Name = "buttonManageSounds";
            this.buttonManageSounds.Size = new System.Drawing.Size(73, 42);
            this.buttonManageSounds.TabIndex = 0;
            this.buttonManageSounds.Text = "Manage sounds";
            this.buttonManageSounds.UseVisualStyleBackColor = true;
            this.buttonManageSounds.Click += new System.EventHandler(this.buttonManageSounds_Click);
            // 
            // trackBarGlobalVolume
            // 
            this.trackBarGlobalVolume.AutoSize = false;
            this.trackBarGlobalVolume.Location = new System.Drawing.Point(56, 21);
            this.trackBarGlobalVolume.Margin = new System.Windows.Forms.Padding(2);
            this.trackBarGlobalVolume.Maximum = 100;
            this.trackBarGlobalVolume.Name = "trackBarGlobalVolume";
            this.trackBarGlobalVolume.Size = new System.Drawing.Size(152, 31);
            this.trackBarGlobalVolume.TabIndex = 12;
            this.trackBarGlobalVolume.Value = 100;
            this.trackBarGlobalVolume.Scroll += new System.EventHandler(this.trackBarGlobalVolume_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Global volume";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 15000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 10;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(9, 93);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(336, 206);
            this.flowLayoutPanel1.TabIndex = 15;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // buttonAddNew
            // 
            this.buttonAddNew.Location = new System.Drawing.Point(9, 66);
            this.buttonAddNew.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAddNew.Name = "buttonAddNew";
            this.buttonAddNew.Size = new System.Drawing.Size(113, 22);
            this.buttonAddNew.TabIndex = 16;
            this.buttonAddNew.Text = "Add new...";
            this.buttonAddNew.UseVisualStyleBackColor = true;
            this.buttonAddNew.Click += new System.EventHandler(this.buttonAddNew_Click);
            // 
            // buttonMute
            // 
            this.buttonMute.BackgroundImage = global::Aldurcraft.WurmOnline.WurmAssistant2.Properties.Resources.SoundEnabledSmall;
            this.buttonMute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonMute.Location = new System.Drawing.Point(9, 10);
            this.buttonMute.Margin = new System.Windows.Forms.Padding(2);
            this.buttonMute.Name = "buttonMute";
            this.buttonMute.Size = new System.Drawing.Size(42, 42);
            this.buttonMute.TabIndex = 9;
            this.buttonMute.UseVisualStyleBackColor = true;
            this.buttonMute.Click += new System.EventHandler(this.buttonMute_Click);
            // 
            // FormTriggersMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 314);
            this.Controls.Add(this.buttonAddNew);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarGlobalVolume);
            this.Controls.Add(this.buttonMute);
            this.Controls.Add(this.buttonManageSounds);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(372, 352);
            this.Name = "FormTriggersMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Triggers";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSoundNotifyMain_FormClosing);
            this.Load += new System.EventHandler(this.FormSoundNotifyMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGlobalVolume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonManageSounds;
        public System.Windows.Forms.Button buttonMute;
        private System.Windows.Forms.TrackBar trackBarGlobalVolume;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button buttonAddNew;

    }
}