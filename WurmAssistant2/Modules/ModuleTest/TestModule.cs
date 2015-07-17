using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Test
{
    public class TestModule : AssistantModule
    {
        TestModuleUI UI;

        public override void Initialize()
        {
            Logger.LogInfo("starting", this);
            UI = new TestModuleUI(this);
        }

        void OnNewLogEvents(object sender, Aldurcraft.WurmOnline.WurmLogsManager.NewLogEntriesEventArgs e)
        {
            if (UI != null)
            {
                try
                {
                    foreach (var entry in e.Entries.AllEntries)
                        foreach (var line in entry.Entries)
                            UI.ShowEvent(line);
                }
                catch (Exception _e)
                {
                    Logger.LogError("issue", this, _e);
                }
            }
        }

        public void Subscribe(string text)
        {
            try
            {
                Aldurcraft.WurmOnline.WurmLogsManager.WurmLogs.SubscribeToLogFeed(text, OnNewLogEvents);
            }
            catch (Exception _e)
            {
                
                Logger.LogError("subscribe failed", this, _e);
            }
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            try
            {
                UI.Show();
            }
            catch
            {
                try
                {
                    UI = new TestModuleUI(this);
                    UI.Show();
                }
                catch (Exception _e)
                {
                    Logger.LogError("UI not working", this, _e);
                }

            }
        }

        public override void Update(bool engineSleeping)
        {
            //Logger.LogDebug("updating", this);
        }

        public override void Stop()
        {
            Logger.LogDebug("stopping", this);
            //AssistantEngine.Modules.RemoveButton(this.GetType());
            UI.Close();
            UI.Dispose();
        }
    }
}
