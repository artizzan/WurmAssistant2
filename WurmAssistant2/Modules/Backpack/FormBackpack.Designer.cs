namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Backpack
{
    partial class FormBackpack
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBackpack));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxToolbeltsPlayer = new System.Windows.Forms.ComboBox();
            this.buttonMakeToolbeltExec = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxToolbeltsPlayer);
            this.groupBox1.Controls.Add(this.buttonMakeToolbeltExec);
            this.groupBox1.Location = new System.Drawing.Point(12, 50);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(202, 95);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Toolbelt setup extractor";
            // 
            // comboBoxToolbeltsPlayer
            // 
            this.comboBoxToolbeltsPlayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolbeltsPlayer.FormattingEnabled = true;
            this.comboBoxToolbeltsPlayer.Location = new System.Drawing.Point(6, 21);
            this.comboBoxToolbeltsPlayer.Name = "comboBoxToolbeltsPlayer";
            this.comboBoxToolbeltsPlayer.Size = new System.Drawing.Size(180, 24);
            this.comboBoxToolbeltsPlayer.TabIndex = 1;
            // 
            // buttonMakeToolbeltExec
            // 
            this.buttonMakeToolbeltExec.Location = new System.Drawing.Point(6, 51);
            this.buttonMakeToolbeltExec.Name = "buttonMakeToolbeltExec";
            this.buttonMakeToolbeltExec.Size = new System.Drawing.Size(180, 26);
            this.buttonMakeToolbeltExec.TabIndex = 0;
            this.buttonMakeToolbeltExec.Text = "Make toolbelt EXEC file";
            this.buttonMakeToolbeltExec.UseVisualStyleBackColor = true;
            this.buttonMakeToolbeltExec.Click += new System.EventHandler(this.buttonMakeToolbeltExec_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(527, 17);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "All the utility stuff below is (or should be) explained in wiki, just click me to" +
    " go there.";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // timer1
            // 
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormBackpack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 203);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormBackpack";
            this.Text = "Backpack";
            this.Load += new System.EventHandler(this.FormBackpack_Load);
            this.Resize += new System.EventHandler(this.FormBackpack_Resize);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBoxToolbeltsPlayer;
        private System.Windows.Forms.Button buttonMakeToolbeltExec;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Timer timer1;
    }
}