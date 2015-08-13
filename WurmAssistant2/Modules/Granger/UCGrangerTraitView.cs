using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;
using BrightIdeasSoftware;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class UCGrangerTraitView : UserControl
    {
        FormGrangerMain MainForm;
        GrangerContext Context;
        TraitViewManager Manager;

        bool _debug_MainFormAssigned = false;
        public UCGrangerTraitView()
        {
            InitializeComponent();
        }

        internal void Init(FormGrangerMain formGrangerMain, GrangerContext context)
        {
            MainForm = formGrangerMain;
            _debug_MainFormAssigned = true;

            if (MainForm.Settings.Value.AdjustForDarkThemes)
            {
                MakeDarkHighContrastFriendly();
            }

            Context = context;
            if (MainForm.Settings.Value.TraitViewState != null) objectListView1.RestoreState(MainForm.Settings.Value.TraitViewState);
            Manager = new TraitViewManager(MainForm, Context, objectListView1);
        }

        private void MakeDarkHighContrastFriendly()
        {
            objectListView1.HeaderUsesThemes = false;
            objectListView1.HeaderFormatStyle = new HeaderFormatStyle()
            {
                Normal = new HeaderStateStyle()
                {
                    ForeColor = Color.Yellow
                },
                Hot = new HeaderStateStyle()
                {
                    ForeColor = Color.Yellow
                },
                Pressed = new HeaderStateStyle()
                {
                    ForeColor = Color.Yellow
                },
            };
        }

        public void SaveStateToSettings()
        {
            if (!_debug_MainFormAssigned && MainForm == null) return;

            try
            {
                MainForm.Settings.Value.TraitViewState = objectListView1.SaveState();
                MainForm.Settings.DelayedSave();
            }
            catch (Exception _e)
            {
                Logger.LogError("Something went wrong when trying to save trait list state, mainform null: " + (MainForm == null), this, _e);
            }
        }

        private void fullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.TraitViewDisplayMode = TraitViewManager.TraitDisplayMode.Full;
        }

        private void compactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.TraitViewDisplayMode = TraitViewManager.TraitDisplayMode.Compact;
        }

        private void shortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.TraitViewDisplayMode = TraitViewManager.TraitDisplayMode.Shortcut;
        }

        private void objectListView1_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            SaveStateToSettings();
        }

        private void objectListView1_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            SaveStateToSettings();
        }
    }
}
