using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility.PopupNotify;

namespace WA2_Test
{
    public partial class LogManager_Test : Form
    {
        Dictionary<string, WeakReference> AllEngineWeakRefs = new Dictionary<string, WeakReference>();

        public LogManager_Test()
        {
            InitializeComponent();
            WurmLogs.AssignSynchronizingObject(this);
            WurmLogs.Enable();
            timer1.Enabled = true;
        }

        private void buttonAddEngine_Click(object sender, EventArgs e)
        {
            LogEngine engine;
            bool result = WurmLogs.CreateAndReturnEngine(textBoxEngineMod.Text, out engine);
            if (result) AllEngineWeakRefs[textBoxEngineMod.Text] = new WeakReference(engine);
            textBoxEngineFeedback.Text = "Create engine '" + textBoxEngineMod.Text + "' = " + result.ToString();
            RefreshEngineList();
        }

        private void buttonRemoveEngine_Click(object sender, EventArgs e)
        {
            bool result = WurmLogs.KillEngine(textBoxEngineMod.Text);
            textBoxEngineFeedback.Text = "Kill engine '" + textBoxEngineMod.Text + "' = " + result.ToString();
            RefreshEngineList();
        }

        void RefreshEngineList()
        {
            listBoxEngines.Items.Clear();
            listBoxEngines.Items.AddRange(WurmLogs.GetAllLogEngines());
        }

        private void buttonSubscribe_Click(object sender, EventArgs e)
        {
            bool result = WurmLogs.SubscribeToLogFeed(textBoxEngineMod.Text, new EventHandler<NewLogEntriesEventArgs>(OnNewLogEvents));
            textBoxEngineFeedback.Text = "SubscribeToLogFeed '" + textBoxEngineMod.Text + "' = " + result.ToString();
        }

        private void buttonUnsubscribe_Click(object sender, EventArgs e)
        {
            bool result = WurmLogs.UnsubscribeFromLogFeed(textBoxEngineMod.Text, new EventHandler<NewLogEntriesEventArgs>(OnNewLogEvents));
            textBoxEngineFeedback.Text = "UnsubscribeFromLogFeed '" + textBoxEngineMod.Text + "' = " + result.ToString();
        }

        public void OnNewLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            textBoxLogMessages.Text += "\r\n NEW FROM: " + e.Entries.PlayerName;
            foreach (var item in e.Entries.AllEntries)
            {
                foreach (var line in item.Entries)
                {
                    textBoxLogMessages.Text += "\r\n";
                    textBoxLogMessages.Text += string.Format("{0} ({1}): {2}", item.LogType, item.PM_Player, line);
                }
            }
        }

        private void listBoxEngines_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxEngineMod.Text = listBoxEngines.Text;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GC.Collect();

            listBoxAliveEngines.Items.Clear();
            foreach (var keyval in AllEngineWeakRefs)
            {
                if (keyval.Value.IsAlive) listBoxAliveEngines.Items.Add(keyval.Key);
            }
        }

        private void tESTSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WurmClientState_Test ui = new WurmClientState_Test();
            ui.Show();
        }

        private void loggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger_Test ui = new Logger_Test();
            ui.Show();
        }

        private void searcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Searcher_Test ui = new Searcher_Test();
            ui.Show();
        }

        private void soundBankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoundBank.OpenSoundBank();
        }

        private void popupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Popup.SetDefaultTitle("default title changed");
            Popup.Schedule("test", 4000);
        }

        private void serverDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerDataTest ui = new ServerDataTest();
            ui.Show();
        }
    }
}
