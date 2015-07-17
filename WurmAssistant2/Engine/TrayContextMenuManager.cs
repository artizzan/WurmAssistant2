using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.Engine
{
    class TrayContextMenuManager
    {
        private ContextMenuStrip _menuStrip;
        private WurmAssistant _parentForm;

        public TrayContextMenuManager(WurmAssistant parent, ContextMenuStrip menuStrip)
        {
            _menuStrip = menuStrip;
            _parentForm = parent;
        }

        public void Rebuild()
        {
            _menuStrip.Items.Clear();
            
            //add menu item to reopen main window
            var tsmi = new ToolStripMenuItem()
            {
                Text = "Show Main Window"
            };
            tsmi.Click += _parentForm.ShowRestore;
            _menuStrip.Items.Add(tsmi);

            _menuStrip.Items.Add(new ToolStripSeparator());

            //get list of active modules and populate
            var activeModules = ModuleManager.GetActiveModules();

            foreach (var assistantModule in activeModules)
            {
                var descriptor = AssistantModuleDescriptors.GetDescriptor(assistantModule.GetType());
                var menuItem = new ToolStripMenuItem {Text = descriptor.Name};
                if (descriptor.IconPath != null)
                {
                    try
                    {
                        menuItem.ImageAlign = ContentAlignment.MiddleLeft;
                        menuItem.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                        menuItem.Image = Image.FromFile(descriptor.IconPath);
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("problem loading module icon", this, _e);
                    }
                }
                menuItem.Click += assistantModule.OpenUI;
                _menuStrip.Items.Add(menuItem);
            }

            _menuStrip.Items.Add(new ToolStripSeparator());

            //add menu item to close WA
            var tsmi2 = new ToolStripMenuItem()
            {
                Text = "Exit"
            };
            tsmi2.Click += _parentForm.CloseConfirm;
            _menuStrip.Items.Add(tsmi2);
        }
    }
}
