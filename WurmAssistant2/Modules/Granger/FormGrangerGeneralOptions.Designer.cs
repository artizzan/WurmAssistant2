namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    partial class FormGrangerGeneralOptions
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
            this.checkBoxAlwaysUpdateUnlessMultiples = new System.Windows.Forms.CheckBox();
            this.timeSpanInputGroomingTime = new Aldurcraft.WinFormsControls.TimeSpanInput();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxUpdateAgeHealthAllEvents = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxAlwaysUpdateUnlessMultiples
            // 
            this.checkBoxAlwaysUpdateUnlessMultiples.AutoSize = true;
            this.checkBoxAlwaysUpdateUnlessMultiples.Location = new System.Drawing.Point(12, 12);
            this.checkBoxAlwaysUpdateUnlessMultiples.Name = "checkBoxAlwaysUpdateUnlessMultiples";
            this.checkBoxAlwaysUpdateUnlessMultiples.Size = new System.Drawing.Size(307, 30);
            this.checkBoxAlwaysUpdateUnlessMultiples.TabIndex = 0;
            this.checkBoxAlwaysUpdateUnlessMultiples.Text = "Always update horse, regardless which herd they are in, \r\nunless multiple horses " +
    "with same name exist in the database";
            this.checkBoxAlwaysUpdateUnlessMultiples.UseVisualStyleBackColor = true;
            // 
            // timeSpanInputGroomingTime
            // 
            this.timeSpanInputGroomingTime.Location = new System.Drawing.Point(12, 98);
            this.timeSpanInputGroomingTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.timeSpanInputGroomingTime.Name = "timeSpanInputGroomingTime";
            this.timeSpanInputGroomingTime.Size = new System.Drawing.Size(231, 45);
            this.timeSpanInputGroomingTime.TabIndex = 2;
            this.timeSpanInputGroomingTime.Value = System.TimeSpan.Parse("00:00:00");
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(221, 175);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(92, 29);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "Accept";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(319, 175);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(92, 29);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(359, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Hide time in grooming column, after this amount of time since last grooming:";
            // 
            // checkBoxUpdateAgeHealthAllEvents
            // 
            this.checkBoxUpdateAgeHealthAllEvents.AutoSize = true;
            this.checkBoxUpdateAgeHealthAllEvents.Location = new System.Drawing.Point(12, 48);
            this.checkBoxUpdateAgeHealthAllEvents.Name = "checkBoxUpdateAgeHealthAllEvents";
            this.checkBoxUpdateAgeHealthAllEvents.Size = new System.Drawing.Size(306, 30);
            this.checkBoxUpdateAgeHealthAllEvents.TabIndex = 6;
            this.checkBoxUpdateAgeHealthAllEvents.Text = "Try to update horse age and health data from any log event\r\n(when unchecked, upda" +
    "tes only when smile-examining)";
            this.checkBoxUpdateAgeHealthAllEvents.UseVisualStyleBackColor = true;
            // 
            // FormGrangerGeneralOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 216);
            this.Controls.Add(this.checkBoxUpdateAgeHealthAllEvents);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.timeSpanInputGroomingTime);
            this.Controls.Add(this.checkBoxAlwaysUpdateUnlessMultiples);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGrangerGeneralOptions";
            this.ShowIcon = false;
            this.Text = "Granger general options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxAlwaysUpdateUnlessMultiples;
        private WinFormsControls.TimeSpanInput timeSpanInputGroomingTime;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxUpdateAgeHealthAllEvents;
    }
}