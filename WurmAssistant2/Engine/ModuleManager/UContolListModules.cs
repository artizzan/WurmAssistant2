using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class UControlListModules : UserControl
    {
        Type moduleType;

        public UControlListModules(AssistantModuleDescriptors.Descriptor descriptor)
        {
            this.moduleType = descriptor.ModuleType;
            InitializeComponent();
            this.groupBoxFeatureName.Text = descriptor.Name;
            this.textBoxModuleDescription.Text = descriptor.Description ?? "No description provided";

            if (descriptor.IconPath != null)
            {
                try
                {
                    this.pictureBoxModuleIcon.Image = Image.FromFile(descriptor.IconPath);
                }
                catch (Exception _e)
                {
                    Logger.LogError("image error", this, _e);
                }
            }

            if (ModuleManager.IsModuleRunning(descriptor.ModuleType))
            {
                checkBoxUseThis.Checked = true;
            }

            this.checkBoxUseThis.CheckedChanged += userOnCheckedChanged;
        }

        void userOnCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseThis.Checked) ModuleManager.Start(this.moduleType);
            else ModuleManager.Stop(this.moduleType);
        }
    }
}
