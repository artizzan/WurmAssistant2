namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    partial class FormSettings
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
            this.checkBoxWebsiteOnMajorUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxChangelogOnUpdate = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxDisableWebFeedScan = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxHideBeerButton = new System.Windows.Forms.CheckBox();
            this.checkBoxConfirmAppExit = new System.Windows.Forms.CheckBox();
            this.checkBoxAlwaysShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.checkBoxMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.checkBoxStartMinimized = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxWebsiteOnMajorUpdate
            // 
            this.checkBoxWebsiteOnMajorUpdate.AutoSize = true;
            this.checkBoxWebsiteOnMajorUpdate.Location = new System.Drawing.Point(4, 17);
            this.checkBoxWebsiteOnMajorUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxWebsiteOnMajorUpdate.Name = "checkBoxWebsiteOnMajorUpdate";
            this.checkBoxWebsiteOnMajorUpdate.Size = new System.Drawing.Size(259, 17);
            this.checkBoxWebsiteOnMajorUpdate.TabIndex = 0;
            this.checkBoxWebsiteOnMajorUpdate.Text = "Open news in default browser after major updates";
            this.checkBoxWebsiteOnMajorUpdate.UseVisualStyleBackColor = true;
            this.checkBoxWebsiteOnMajorUpdate.CheckedChanged += new System.EventHandler(this.checkBoxWebsiteOnMajorUpdate_CheckedChanged);
            // 
            // checkBoxChangelogOnUpdate
            // 
            this.checkBoxChangelogOnUpdate.AutoSize = true;
            this.checkBoxChangelogOnUpdate.Location = new System.Drawing.Point(4, 39);
            this.checkBoxChangelogOnUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxChangelogOnUpdate.Name = "checkBoxChangelogOnUpdate";
            this.checkBoxChangelogOnUpdate.Size = new System.Drawing.Size(195, 17);
            this.checkBoxChangelogOnUpdate.TabIndex = 1;
            this.checkBoxChangelogOnUpdate.Text = "Show changelog after every update";
            this.checkBoxChangelogOnUpdate.UseVisualStyleBackColor = true;
            this.checkBoxChangelogOnUpdate.CheckedChanged += new System.EventHandler(this.checkBoxChangelogOnUpdate_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxWebsiteOnMajorUpdate);
            this.groupBox1.Controls.Add(this.checkBoxChangelogOnUpdate);
            this.groupBox1.Location = new System.Drawing.Point(16, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(318, 61);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "App updates";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(359, 233);
            this.panel1.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(357, 231);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox3);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(351, 225);
            this.panel2.TabIndex = 4;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxDisableWebFeedScan);
            this.groupBox3.Location = new System.Drawing.Point(16, 169);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(318, 41);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Global settings";
            // 
            // checkBoxDisableWebFeedScan
            // 
            this.checkBoxDisableWebFeedScan.AutoSize = true;
            this.checkBoxDisableWebFeedScan.Location = new System.Drawing.Point(5, 18);
            this.checkBoxDisableWebFeedScan.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxDisableWebFeedScan.Name = "checkBoxDisableWebFeedScan";
            this.checkBoxDisableWebFeedScan.Size = new System.Drawing.Size(220, 17);
            this.checkBoxDisableWebFeedScan.TabIndex = 5;
            this.checkBoxDisableWebFeedScan.Text = "Disable scanning official wurm web feeds";
            this.checkBoxDisableWebFeedScan.UseVisualStyleBackColor = true;
            this.checkBoxDisableWebFeedScan.CheckedChanged += new System.EventHandler(this.checkBoxDisableWebFeedScan_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxHideBeerButton);
            this.groupBox2.Controls.Add(this.checkBoxConfirmAppExit);
            this.groupBox2.Controls.Add(this.checkBoxAlwaysShowTrayIcon);
            this.groupBox2.Controls.Add(this.checkBoxMinimizeToTray);
            this.groupBox2.Controls.Add(this.checkBoxStartMinimized);
            this.groupBox2.Location = new System.Drawing.Point(16, 72);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(318, 92);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Main window";
            // 
            // checkBoxHideBeerButton
            // 
            this.checkBoxHideBeerButton.AutoSize = true;
            this.checkBoxHideBeerButton.Location = new System.Drawing.Point(4, 70);
            this.checkBoxHideBeerButton.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxHideBeerButton.Name = "checkBoxHideBeerButton";
            this.checkBoxHideBeerButton.Size = new System.Drawing.Size(116, 17);
            this.checkBoxHideBeerButton.TabIndex = 4;
            this.checkBoxHideBeerButton.Text = "Hide \"Beer\" button";
            this.checkBoxHideBeerButton.UseVisualStyleBackColor = true;
            this.checkBoxHideBeerButton.CheckedChanged += new System.EventHandler(this.checkBoxHideBeerButton_CheckedChanged);
            // 
            // checkBoxConfirmAppExit
            // 
            this.checkBoxConfirmAppExit.AutoSize = true;
            this.checkBoxConfirmAppExit.Location = new System.Drawing.Point(151, 39);
            this.checkBoxConfirmAppExit.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxConfirmAppExit.Name = "checkBoxConfirmAppExit";
            this.checkBoxConfirmAppExit.Size = new System.Drawing.Size(116, 17);
            this.checkBoxConfirmAppExit.TabIndex = 3;
            this.checkBoxConfirmAppExit.Text = "Confirm on app exit";
            this.checkBoxConfirmAppExit.UseVisualStyleBackColor = true;
            this.checkBoxConfirmAppExit.CheckedChanged += new System.EventHandler(this.checkBoxConfirmAppExit_CheckedChanged);
            // 
            // checkBoxAlwaysShowTrayIcon
            // 
            this.checkBoxAlwaysShowTrayIcon.AutoSize = true;
            this.checkBoxAlwaysShowTrayIcon.Location = new System.Drawing.Point(151, 17);
            this.checkBoxAlwaysShowTrayIcon.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxAlwaysShowTrayIcon.Name = "checkBoxAlwaysShowTrayIcon";
            this.checkBoxAlwaysShowTrayIcon.Size = new System.Drawing.Size(130, 17);
            this.checkBoxAlwaysShowTrayIcon.TabIndex = 2;
            this.checkBoxAlwaysShowTrayIcon.Text = "Always show tray icon";
            this.checkBoxAlwaysShowTrayIcon.UseVisualStyleBackColor = true;
            this.checkBoxAlwaysShowTrayIcon.CheckedChanged += new System.EventHandler(this.checkBoxAlwaysShowTrayIcon_CheckedChanged);
            // 
            // checkBoxMinimizeToTray
            // 
            this.checkBoxMinimizeToTray.AutoSize = true;
            this.checkBoxMinimizeToTray.Location = new System.Drawing.Point(4, 17);
            this.checkBoxMinimizeToTray.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            this.checkBoxMinimizeToTray.Size = new System.Drawing.Size(98, 17);
            this.checkBoxMinimizeToTray.TabIndex = 0;
            this.checkBoxMinimizeToTray.Text = "Minimize to tray";
            this.checkBoxMinimizeToTray.UseVisualStyleBackColor = true;
            this.checkBoxMinimizeToTray.CheckedChanged += new System.EventHandler(this.checkBoxMinimizeToTray_CheckedChanged);
            // 
            // checkBoxStartMinimized
            // 
            this.checkBoxStartMinimized.AutoSize = true;
            this.checkBoxStartMinimized.Location = new System.Drawing.Point(4, 39);
            this.checkBoxStartMinimized.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxStartMinimized.Name = "checkBoxStartMinimized";
            this.checkBoxStartMinimized.Size = new System.Drawing.Size(96, 17);
            this.checkBoxStartMinimized.TabIndex = 1;
            this.checkBoxStartMinimized.Text = "Start minimized";
            this.checkBoxStartMinimized.UseVisualStyleBackColor = true;
            this.checkBoxStartMinimized.CheckedChanged += new System.EventHandler(this.checkBoxStartMinimized_CheckedChanged);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 233);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Assistant Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxWebsiteOnMajorUpdate;
        private System.Windows.Forms.CheckBox checkBoxChangelogOnUpdate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxMinimizeToTray;
        private System.Windows.Forms.CheckBox checkBoxStartMinimized;
        private System.Windows.Forms.CheckBox checkBoxAlwaysShowTrayIcon;
        private System.Windows.Forms.CheckBox checkBoxConfirmAppExit;
        private System.Windows.Forms.CheckBox checkBoxHideBeerButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxDisableWebFeedScan;
    }
}