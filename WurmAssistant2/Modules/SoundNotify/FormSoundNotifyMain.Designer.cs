namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    partial class FormSoundNotifyMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSoundNotifyMain));
            this.buttonManageSounds = new System.Windows.Forms.Button();
            this.trackBarGlobalVolume = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonAddNew = new System.Windows.Forms.Button();
            this.buttonMute = new System.Windows.Forms.Button();
            this.buttonAdvancedOptions = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonModifyQueueTriggers = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGlobalVolume)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonManageSounds
            // 
            this.buttonManageSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonManageSounds.Location = new System.Drawing.Point(363, 9);
            this.buttonManageSounds.Name = "buttonManageSounds";
            this.buttonManageSounds.Size = new System.Drawing.Size(97, 40);
            this.buttonManageSounds.TabIndex = 0;
            this.buttonManageSounds.Text = "My sounds";
            this.buttonManageSounds.UseVisualStyleBackColor = true;
            this.buttonManageSounds.Click += new System.EventHandler(this.buttonManageSounds_Click);
            // 
            // trackBarGlobalVolume
            // 
            this.trackBarGlobalVolume.AutoSize = false;
            this.trackBarGlobalVolume.Location = new System.Drawing.Point(74, 26);
            this.trackBarGlobalVolume.Maximum = 100;
            this.trackBarGlobalVolume.Name = "trackBarGlobalVolume";
            this.trackBarGlobalVolume.Size = new System.Drawing.Size(202, 38);
            this.trackBarGlobalVolume.TabIndex = 12;
            this.trackBarGlobalVolume.Value = 100;
            this.trackBarGlobalVolume.Scroll += new System.EventHandler(this.trackBarGlobalVolume_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
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
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 114);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(448, 254);
            this.flowLayoutPanel1.TabIndex = 15;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // buttonAddNew
            // 
            this.buttonAddNew.Location = new System.Drawing.Point(12, 81);
            this.buttonAddNew.Name = "buttonAddNew";
            this.buttonAddNew.Size = new System.Drawing.Size(151, 27);
            this.buttonAddNew.TabIndex = 16;
            this.buttonAddNew.Text = "Add new...";
            this.buttonAddNew.UseVisualStyleBackColor = true;
            this.buttonAddNew.Click += new System.EventHandler(this.buttonAddNew_Click);
            // 
            // buttonMute
            // 
            this.buttonMute.BackgroundImage = global::Aldurcraft.WurmOnline.WurmAssistant2.Properties.Resources.SoundEnabledSmall;
            this.buttonMute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonMute.Location = new System.Drawing.Point(12, 12);
            this.buttonMute.Name = "buttonMute";
            this.buttonMute.Size = new System.Drawing.Size(56, 52);
            this.buttonMute.TabIndex = 9;
            this.buttonMute.UseVisualStyleBackColor = true;
            this.buttonMute.Click += new System.EventHandler(this.buttonMute_Click);
            // 
            // buttonAdvancedOptions
            // 
            this.buttonAdvancedOptions.Location = new System.Drawing.Point(363, 55);
            this.buttonAdvancedOptions.Name = "buttonAdvancedOptions";
            this.buttonAdvancedOptions.Size = new System.Drawing.Size(97, 53);
            this.buttonAdvancedOptions.TabIndex = 17;
            this.buttonAdvancedOptions.Text = "Advanced options";
            this.buttonAdvancedOptions.UseVisualStyleBackColor = true;
            this.buttonAdvancedOptions.Click += new System.EventHandler(this.buttonAdvancedOptions_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.buttonModifyQueueTriggers);
            this.panel1.Location = new System.Drawing.Point(282, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(75, 99);
            this.panel1.TabIndex = 18;
            this.panel1.Visible = false;
            // 
            // buttonModifyQueueTriggers
            // 
            this.buttonModifyQueueTriggers.Location = new System.Drawing.Point(3, 3);
            this.buttonModifyQueueTriggers.Name = "buttonModifyQueueTriggers";
            this.buttonModifyQueueTriggers.Size = new System.Drawing.Size(67, 91);
            this.buttonModifyQueueTriggers.TabIndex = 14;
            this.buttonModifyQueueTriggers.Text = "Modify queue sound triggers";
            this.buttonModifyQueueTriggers.UseVisualStyleBackColor = true;
            this.buttonModifyQueueTriggers.Click += new System.EventHandler(this.buttonModifyQueueTriggers_Click);
            // 
            // FormSoundNotifyMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 380);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonAdvancedOptions);
            this.Controls.Add(this.buttonAddNew);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarGlobalVolume);
            this.Controls.Add(this.buttonMute);
            this.Controls.Add(this.buttonManageSounds);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(490, 425);
            this.Name = "FormSoundNotifyMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sound Triggers";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSoundNotifyMain_FormClosing);
            this.Load += new System.EventHandler(this.FormSoundNotifyMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGlobalVolume)).EndInit();
            this.panel1.ResumeLayout(false);
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
        private System.Windows.Forms.Button buttonAdvancedOptions;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonModifyQueueTriggers;

    }
}