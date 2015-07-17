namespace WA2_Test
{
    partial class WurmClientState_Test
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
            this.textBoxInitResult = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOverrideWurmDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonApplyWurmDirOverride = new System.Windows.Forms.Button();
            this.buttonGetIintState = new System.Windows.Forms.Button();
            this.buttonReinit = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.listBoxPlayers = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxConfForPlayer = new System.Windows.Forms.TextBox();
            this.textBoxSettings = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.listBoxChangeLogging = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listBoxChangeSkillgain = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker3 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker4 = new System.ComponentModel.BackgroundWorker();
            this.button10 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBoxInitResult
            // 
            this.textBoxInitResult.Location = new System.Drawing.Point(12, 36);
            this.textBoxInitResult.Name = "textBoxInitResult";
            this.textBoxInitResult.ReadOnly = true;
            this.textBoxInitResult.Size = new System.Drawing.Size(166, 22);
            this.textBoxInitResult.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Initialize result";
            // 
            // textBoxOverrideWurmDir
            // 
            this.textBoxOverrideWurmDir.Location = new System.Drawing.Point(212, 36);
            this.textBoxOverrideWurmDir.Name = "textBoxOverrideWurmDir";
            this.textBoxOverrideWurmDir.Size = new System.Drawing.Size(484, 22);
            this.textBoxOverrideWurmDir.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(213, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "override wurm dir";
            // 
            // buttonApplyWurmDirOverride
            // 
            this.buttonApplyWurmDirOverride.Location = new System.Drawing.Point(212, 64);
            this.buttonApplyWurmDirOverride.Name = "buttonApplyWurmDirOverride";
            this.buttonApplyWurmDirOverride.Size = new System.Drawing.Size(75, 23);
            this.buttonApplyWurmDirOverride.TabIndex = 4;
            this.buttonApplyWurmDirOverride.Text = "Apply";
            this.buttonApplyWurmDirOverride.UseVisualStyleBackColor = true;
            this.buttonApplyWurmDirOverride.Click += new System.EventHandler(this.buttonApplyWurmDirOverride_Click);
            // 
            // buttonGetIintState
            // 
            this.buttonGetIintState.Location = new System.Drawing.Point(12, 64);
            this.buttonGetIintState.Name = "buttonGetIintState";
            this.buttonGetIintState.Size = new System.Drawing.Size(75, 23);
            this.buttonGetIintState.TabIndex = 5;
            this.buttonGetIintState.Text = "Get";
            this.buttonGetIintState.UseVisualStyleBackColor = true;
            this.buttonGetIintState.Click += new System.EventHandler(this.buttonGetIintState_Click);
            // 
            // buttonReinit
            // 
            this.buttonReinit.Location = new System.Drawing.Point(12, 93);
            this.buttonReinit.Name = "buttonReinit";
            this.buttonReinit.Size = new System.Drawing.Size(75, 23);
            this.buttonReinit.TabIndex = 6;
            this.buttonReinit.Text = "Reinit";
            this.buttonReinit.UseVisualStyleBackColor = true;
            this.buttonReinit.Click += new System.EventHandler(this.buttonReinit_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 142);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Players";
            // 
            // listBoxPlayers
            // 
            this.listBoxPlayers.FormattingEnabled = true;
            this.listBoxPlayers.ItemHeight = 16;
            this.listBoxPlayers.Location = new System.Drawing.Point(12, 162);
            this.listBoxPlayers.Name = "listBoxPlayers";
            this.listBoxPlayers.Size = new System.Drawing.Size(120, 132);
            this.listBoxPlayers.TabIndex = 11;
            this.listBoxPlayers.SelectedIndexChanged += new System.EventHandler(this.listBoxPlayers_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(248, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(142, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Config for this player:";
            // 
            // textBoxConfForPlayer
            // 
            this.textBoxConfForPlayer.Location = new System.Drawing.Point(396, 139);
            this.textBoxConfForPlayer.Name = "textBoxConfForPlayer";
            this.textBoxConfForPlayer.Size = new System.Drawing.Size(141, 22);
            this.textBoxConfForPlayer.TabIndex = 13;
            // 
            // textBoxSettings
            // 
            this.textBoxSettings.Location = new System.Drawing.Point(251, 179);
            this.textBoxSettings.Multiline = true;
            this.textBoxSettings.Name = "textBoxSettings";
            this.textBoxSettings.Size = new System.Drawing.Size(427, 354);
            this.textBoxSettings.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(248, 159);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 17);
            this.label7.TabIndex = 16;
            this.label7.Text = "Settings:";
            // 
            // listBoxChangeLogging
            // 
            this.listBoxChangeLogging.FormattingEnabled = true;
            this.listBoxChangeLogging.ItemHeight = 16;
            this.listBoxChangeLogging.Location = new System.Drawing.Point(704, 179);
            this.listBoxChangeLogging.Name = "listBoxChangeLogging";
            this.listBoxChangeLogging.Size = new System.Drawing.Size(215, 84);
            this.listBoxChangeLogging.TabIndex = 17;
            this.listBoxChangeLogging.SelectedIndexChanged += new System.EventHandler(this.listBoxChangeLogging_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(704, 391);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 50);
            this.button1.TabIndex = 18;
            this.button1.Text = "alignment false";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(816, 391);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 50);
            this.button2.TabIndex = 19;
            this.button2.Text = "alignment true";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // listBoxChangeSkillgain
            // 
            this.listBoxChangeSkillgain.FormattingEnabled = true;
            this.listBoxChangeSkillgain.ItemHeight = 16;
            this.listBoxChangeSkillgain.Location = new System.Drawing.Point(704, 269);
            this.listBoxChangeSkillgain.Name = "listBoxChangeSkillgain";
            this.listBoxChangeSkillgain.Size = new System.Drawing.Size(215, 116);
            this.listBoxChangeSkillgain.TabIndex = 20;
            this.listBoxChangeSkillgain.SelectedIndexChanged += new System.EventHandler(this.listBoxChangeSkillgain_SelectedIndexChanged);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(704, 447);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(106, 50);
            this.button3.TabIndex = 21;
            this.button3.Text = "favor false";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(816, 447);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(103, 50);
            this.button4.TabIndex = 22;
            this.button4.Text = "favor false";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(704, 503);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(106, 50);
            this.button5.TabIndex = 23;
            this.button5.Text = "skills on quit false";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(816, 503);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(106, 50);
            this.button6.TabIndex = 24;
            this.button6.Text = "skills on quit true";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(704, 559);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(106, 50);
            this.button7.TabIndex = 25;
            this.button7.Text = "timestamp false";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(816, 559);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(106, 50);
            this.button8.TabIndex = 26;
            this.button8.Text = "timestamp true";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(251, 573);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(108, 56);
            this.button9.TabIndex = 27;
            this.button9.Text = "StressTest";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            // 
            // backgroundWorker3
            // 
            this.backgroundWorker3.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker3_DoWork);
            // 
            // backgroundWorker4
            // 
            this.backgroundWorker4.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker4_DoWork);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(365, 573);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(108, 56);
            this.button10.TabIndex = 28;
            this.button10.Text = "StressTest2";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // WurmClientState_Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 768);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.listBoxChangeSkillgain);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBoxChangeLogging);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxSettings);
            this.Controls.Add(this.textBoxConfForPlayer);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.listBoxPlayers);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonReinit);
            this.Controls.Add(this.buttonGetIintState);
            this.Controls.Add(this.buttonApplyWurmDirOverride);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxOverrideWurmDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxInitResult);
            this.Name = "WurmClientState_Test";
            this.Text = "WurmClientState Test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxInitResult;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOverrideWurmDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonApplyWurmDirOverride;
        private System.Windows.Forms.Button buttonGetIintState;
        private System.Windows.Forms.Button buttonReinit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBoxPlayers;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxConfForPlayer;
        private System.Windows.Forms.TextBox textBoxSettings;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox listBoxChangeLogging;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox listBoxChangeSkillgain;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.ComponentModel.BackgroundWorker backgroundWorker3;
        private System.ComponentModel.BackgroundWorker backgroundWorker4;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Timer timer1;
    }
}