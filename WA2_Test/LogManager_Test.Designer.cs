namespace WA2_Test
{
    partial class LogManager_Test
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageLogManager = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.listBoxAliveEngines = new System.Windows.Forms.ListBox();
            this.buttonUnsubscribe = new System.Windows.Forms.Button();
            this.textBoxEngineFeedback = new System.Windows.Forms.TextBox();
            this.buttonSubscribe = new System.Windows.Forms.Button();
            this.buttonRemoveEngine = new System.Windows.Forms.Button();
            this.buttonAddEngine = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxEngineMod = new System.Windows.Forms.TextBox();
            this.textBoxLogMessages = new System.Windows.Forms.TextBox();
            this.listBoxEngines = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tESTSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searcherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundBankToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.popupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serverDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPageLogManager.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageLogManager);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1101, 639);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageLogManager
            // 
            this.tabPageLogManager.Controls.Add(this.label4);
            this.tabPageLogManager.Controls.Add(this.listBoxAliveEngines);
            this.tabPageLogManager.Controls.Add(this.buttonUnsubscribe);
            this.tabPageLogManager.Controls.Add(this.textBoxEngineFeedback);
            this.tabPageLogManager.Controls.Add(this.buttonSubscribe);
            this.tabPageLogManager.Controls.Add(this.buttonRemoveEngine);
            this.tabPageLogManager.Controls.Add(this.buttonAddEngine);
            this.tabPageLogManager.Controls.Add(this.label3);
            this.tabPageLogManager.Controls.Add(this.label2);
            this.tabPageLogManager.Controls.Add(this.textBoxEngineMod);
            this.tabPageLogManager.Controls.Add(this.textBoxLogMessages);
            this.tabPageLogManager.Controls.Add(this.listBoxEngines);
            this.tabPageLogManager.Controls.Add(this.label1);
            this.tabPageLogManager.Location = new System.Drawing.Point(4, 25);
            this.tabPageLogManager.Name = "tabPageLogManager";
            this.tabPageLogManager.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogManager.Size = new System.Drawing.Size(1093, 610);
            this.tabPageLogManager.TabIndex = 0;
            this.tabPageLogManager.Text = "Log Manager";
            this.tabPageLogManager.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(433, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "Alive engines";
            // 
            // listBoxAliveEngines
            // 
            this.listBoxAliveEngines.FormattingEnabled = true;
            this.listBoxAliveEngines.ItemHeight = 16;
            this.listBoxAliveEngines.Location = new System.Drawing.Point(433, 23);
            this.listBoxAliveEngines.Name = "listBoxAliveEngines";
            this.listBoxAliveEngines.Size = new System.Drawing.Size(161, 276);
            this.listBoxAliveEngines.TabIndex = 11;
            // 
            // buttonUnsubscribe
            // 
            this.buttonUnsubscribe.Location = new System.Drawing.Point(191, 152);
            this.buttonUnsubscribe.Name = "buttonUnsubscribe";
            this.buttonUnsubscribe.Size = new System.Drawing.Size(203, 23);
            this.buttonUnsubscribe.TabIndex = 10;
            this.buttonUnsubscribe.Text = "Unsubscribe";
            this.buttonUnsubscribe.UseVisualStyleBackColor = true;
            this.buttonUnsubscribe.Click += new System.EventHandler(this.buttonUnsubscribe_Click);
            // 
            // textBoxEngineFeedback
            // 
            this.textBoxEngineFeedback.Location = new System.Drawing.Point(191, 194);
            this.textBoxEngineFeedback.Multiline = true;
            this.textBoxEngineFeedback.Name = "textBoxEngineFeedback";
            this.textBoxEngineFeedback.ReadOnly = true;
            this.textBoxEngineFeedback.Size = new System.Drawing.Size(203, 85);
            this.textBoxEngineFeedback.TabIndex = 9;
            // 
            // buttonSubscribe
            // 
            this.buttonSubscribe.Location = new System.Drawing.Point(191, 123);
            this.buttonSubscribe.Name = "buttonSubscribe";
            this.buttonSubscribe.Size = new System.Drawing.Size(203, 23);
            this.buttonSubscribe.TabIndex = 8;
            this.buttonSubscribe.Text = "Subscribe";
            this.buttonSubscribe.UseVisualStyleBackColor = true;
            this.buttonSubscribe.Click += new System.EventHandler(this.buttonSubscribe_Click);
            // 
            // buttonRemoveEngine
            // 
            this.buttonRemoveEngine.Location = new System.Drawing.Point(191, 94);
            this.buttonRemoveEngine.Name = "buttonRemoveEngine";
            this.buttonRemoveEngine.Size = new System.Drawing.Size(203, 23);
            this.buttonRemoveEngine.TabIndex = 7;
            this.buttonRemoveEngine.Text = "Remove";
            this.buttonRemoveEngine.UseVisualStyleBackColor = true;
            this.buttonRemoveEngine.Click += new System.EventHandler(this.buttonRemoveEngine_Click);
            // 
            // buttonAddEngine
            // 
            this.buttonAddEngine.Location = new System.Drawing.Point(191, 65);
            this.buttonAddEngine.Name = "buttonAddEngine";
            this.buttonAddEngine.Size = new System.Drawing.Size(203, 23);
            this.buttonAddEngine.TabIndex = 6;
            this.buttonAddEngine.Text = "Add";
            this.buttonAddEngine.UseVisualStyleBackColor = true;
            this.buttonAddEngine.Click += new System.EventHandler(this.buttonAddEngine_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(188, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "add/remove engine";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 330);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "log messages";
            // 
            // textBoxEngineMod
            // 
            this.textBoxEngineMod.Location = new System.Drawing.Point(191, 37);
            this.textBoxEngineMod.Name = "textBoxEngineMod";
            this.textBoxEngineMod.Size = new System.Drawing.Size(203, 22);
            this.textBoxEngineMod.TabIndex = 3;
            // 
            // textBoxLogMessages
            // 
            this.textBoxLogMessages.Location = new System.Drawing.Point(3, 350);
            this.textBoxLogMessages.Multiline = true;
            this.textBoxLogMessages.Name = "textBoxLogMessages";
            this.textBoxLogMessages.ReadOnly = true;
            this.textBoxLogMessages.Size = new System.Drawing.Size(1084, 245);
            this.textBoxLogMessages.TabIndex = 2;
            // 
            // listBoxEngines
            // 
            this.listBoxEngines.FormattingEnabled = true;
            this.listBoxEngines.ItemHeight = 16;
            this.listBoxEngines.Location = new System.Drawing.Point(9, 23);
            this.listBoxEngines.Name = "listBoxEngines";
            this.listBoxEngines.Size = new System.Drawing.Size(151, 276);
            this.listBoxEngines.TabIndex = 1;
            this.listBoxEngines.SelectedIndexChanged += new System.EventHandler(this.listBoxEngines_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Active Engines";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1093, 610);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tESTSToolStripMenuItem,
            this.loggerToolStripMenuItem,
            this.searcherToolStripMenuItem,
            this.soundBankToolStripMenuItem,
            this.popupToolStripMenuItem,
            this.serverDataToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1125, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tESTSToolStripMenuItem
            // 
            this.tESTSToolStripMenuItem.Name = "tESTSToolStripMenuItem";
            this.tESTSToolStripMenuItem.Size = new System.Drawing.Size(133, 24);
            this.tESTSToolStripMenuItem.Text = "WurmClientState";
            this.tESTSToolStripMenuItem.Click += new System.EventHandler(this.tESTSToolStripMenuItem_Click);
            // 
            // loggerToolStripMenuItem
            // 
            this.loggerToolStripMenuItem.Name = "loggerToolStripMenuItem";
            this.loggerToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.loggerToolStripMenuItem.Text = "Logger";
            this.loggerToolStripMenuItem.Click += new System.EventHandler(this.loggerToolStripMenuItem_Click);
            // 
            // searcherToolStripMenuItem
            // 
            this.searcherToolStripMenuItem.Name = "searcherToolStripMenuItem";
            this.searcherToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.searcherToolStripMenuItem.Text = "Searcher";
            this.searcherToolStripMenuItem.Click += new System.EventHandler(this.searcherToolStripMenuItem_Click);
            // 
            // soundBankToolStripMenuItem
            // 
            this.soundBankToolStripMenuItem.Name = "soundBankToolStripMenuItem";
            this.soundBankToolStripMenuItem.Size = new System.Drawing.Size(95, 24);
            this.soundBankToolStripMenuItem.Text = "SoundBank";
            this.soundBankToolStripMenuItem.Click += new System.EventHandler(this.soundBankToolStripMenuItem_Click);
            // 
            // popupToolStripMenuItem
            // 
            this.popupToolStripMenuItem.Name = "popupToolStripMenuItem";
            this.popupToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.popupToolStripMenuItem.Text = "Popup";
            this.popupToolStripMenuItem.Click += new System.EventHandler(this.popupToolStripMenuItem_Click);
            // 
            // serverDataToolStripMenuItem
            // 
            this.serverDataToolStripMenuItem.Name = "serverDataToolStripMenuItem";
            this.serverDataToolStripMenuItem.Size = new System.Drawing.Size(94, 24);
            this.serverDataToolStripMenuItem.Text = "ServerData";
            this.serverDataToolStripMenuItem.Click += new System.EventHandler(this.serverDataToolStripMenuItem_Click);
            // 
            // LogManager_Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1125, 678);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "LogManager_Test";
            this.Text = "Log Manager unit test";
            this.tabControl1.ResumeLayout(false);
            this.tabPageLogManager.ResumeLayout(false);
            this.tabPageLogManager.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageLogManager;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxEngineMod;
        private System.Windows.Forms.TextBox textBoxLogMessages;
        private System.Windows.Forms.ListBox listBoxEngines;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxEngineFeedback;
        private System.Windows.Forms.Button buttonSubscribe;
        private System.Windows.Forms.Button buttonRemoveEngine;
        private System.Windows.Forms.Button buttonAddEngine;
        private System.Windows.Forms.Button buttonUnsubscribe;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBoxAliveEngines;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tESTSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searcherToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem soundBankToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem popupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverDataToolStripMenuItem;
    }
}

