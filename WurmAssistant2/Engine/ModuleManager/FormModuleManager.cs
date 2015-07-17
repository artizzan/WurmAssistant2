using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class FormModuleManager : Form
    {
        public FormModuleManager()
        {
            InitializeComponent();
        }

        private void FormModuleManager_Load(object sender, EventArgs e)
        {
            AssistantModuleDescriptors.Descriptor[] descriptors = AssistantModuleDescriptors.GetAllDescriptors();

            foreach (var descriptor in descriptors)
            {
                flowLayoutPanel1.Controls.Add(new UControlListModules(descriptor));
            }
        }
    }
}
