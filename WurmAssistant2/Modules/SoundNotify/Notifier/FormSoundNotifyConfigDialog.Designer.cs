namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    partial class FormSoundNotifyConfigDialog
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
            this.textBoxChooseCond = new System.Windows.Forms.TextBox();
            this.checkBoxUseRegexSemantics = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.listBoxChooseSound = new System.Windows.Forms.ListBox();
            this.checkedListBoxSearchIn = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonCheckAll = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // textBoxChooseCond
            // 
            this.textBoxChooseCond.Location = new System.Drawing.Point(12, 213);
            this.textBoxChooseCond.Name = "textBoxChooseCond";
            this.textBoxChooseCond.Size = new System.Drawing.Size(327, 22);
            this.textBoxChooseCond.TabIndex = 2;
            this.textBoxChooseCond.TextChanged += new System.EventHandler(this.textBoxChooseCond_TextChanged);
            // 
            // checkBoxUseRegexSemantics
            // 
            this.checkBoxUseRegexSemantics.AutoSize = true;
            this.checkBoxUseRegexSemantics.Location = new System.Drawing.Point(12, 250);
            this.checkBoxUseRegexSemantics.Name = "checkBoxUseRegexSemantics";
            this.checkBoxUseRegexSemantics.Size = new System.Drawing.Size(166, 38);
            this.checkBoxUseRegexSemantics.TabIndex = 3;
            this.checkBoxUseRegexSemantics.Text = "Use Regex semantics\r\n(advanced option)";
            this.checkBoxUseRegexSemantics.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Play this sound:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 193);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(261, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "When this text is found (case sensitive!):";
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(15, 342);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(107, 37);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(128, 342);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(107, 37);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(284, 9);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(55, 24);
            this.buttonPlay.TabIndex = 1;
            this.buttonPlay.Text = "Play";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // listBoxChooseSound
            // 
            this.listBoxChooseSound.FormattingEnabled = true;
            this.listBoxChooseSound.ItemHeight = 16;
            this.listBoxChooseSound.Location = new System.Drawing.Point(12, 35);
            this.listBoxChooseSound.Name = "listBoxChooseSound";
            this.listBoxChooseSound.Size = new System.Drawing.Size(327, 148);
            this.listBoxChooseSound.TabIndex = 0;
            this.listBoxChooseSound.DoubleClick += new System.EventHandler(this.listBoxChooseSound_DoubleClick);
            // 
            // checkedListBoxSearchIn
            // 
            this.checkedListBoxSearchIn.CheckOnClick = true;
            this.checkedListBoxSearchIn.FormattingEnabled = true;
            this.checkedListBoxSearchIn.Location = new System.Drawing.Point(366, 35);
            this.checkedListBoxSearchIn.Name = "checkedListBoxSearchIn";
            this.checkedListBoxSearchIn.Size = new System.Drawing.Size(303, 344);
            this.checkedListBoxSearchIn.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(363, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "In these logs:";
            // 
            // buttonCheckAll
            // 
            this.buttonCheckAll.Location = new System.Drawing.Point(579, 9);
            this.buttonCheckAll.Name = "buttonCheckAll";
            this.buttonCheckAll.Size = new System.Drawing.Size(90, 24);
            this.buttonCheckAll.TabIndex = 5;
            this.buttonCheckAll.Text = "Select All";
            this.buttonCheckAll.UseVisualStyleBackColor = true;
            this.buttonCheckAll.Click += new System.EventHandler(this.buttonCheckAll_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 15000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 10;
            // 
            // FormSoundNotifyConfigDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(701, 401);
            this.Controls.Add(this.buttonCheckAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkedListBoxSearchIn);
            this.Controls.Add(this.listBoxChooseSound);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxUseRegexSemantics);
            this.Controls.Add(this.textBoxChooseCond);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSoundNotifyConfigDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add new custom trigger";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormSoundNotifyConfigDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        public System.Windows.Forms.TextBox textBoxChooseCond;
        public System.Windows.Forms.CheckBox checkBoxUseRegexSemantics;
        private System.Windows.Forms.Button buttonPlay;
        public System.Windows.Forms.ListBox listBoxChooseSound;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckedListBox checkedListBoxSearchIn;
        private System.Windows.Forms.Button buttonCheckAll;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}