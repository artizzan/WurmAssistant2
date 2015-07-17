using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    public class ModuleSoundNotify : AssistantModule
    {
        [DataContract]
        public class SoundNotifySettings
        {
            [DataMember]
            public HashSet<string> ActiveCharacterNames = new HashSet<string>();
            [DataMember]
            public bool GlobalMute = false;
            [DataMember]
            float globalVolume = 1.0F;
            public float GlobalVolume
            {
                get { return globalVolume; }
                set
                {
                    globalVolume = GeneralHelper.Validate<float>(value, 0.0F, 1.0F);
                    SoundBank.ChangeGlobalVolume(globalVolume);
                }
            }
        }

        FormSoundNotifyMain MainUI;
        Dictionary<string, SoundNotifier> AllNotifiers = new Dictionary<string, SoundNotifier>();
        public PersistentObject<SoundNotifySettings> Settings;

        public override void Initialize()
        {
            base.Initialize();
            Settings = new PersistentObject<SoundNotifySettings>(new SoundNotifySettings());
            Settings.FilePath = Path.Combine(this.ModuleDataDir, "settings.xml");
            if (!Settings.Load())
            {
                Settings.Save();
            }

            const string queueSoundModFileName = "QueueSoundMod.txt";
            LogQueueParseHelper.Build(
                Path.Combine(this.ModuleDataDir, queueSoundModFileName), 
                Path.Combine(this.ModuleAssetDir, queueSoundModFileName));

            SoundBank.ChangeGlobalVolume(Settings.Value.GlobalVolume);
            MainUI = new FormSoundNotifyMain(this);
            string[] activePlayers = Settings.Value.ActiveCharacterNames.ToArray();
            foreach (var name in activePlayers)
            {
                AddNotifier(name);
            }
            
        }

        public override void Update(bool engineSleeping)
        {
            Settings.Update();
            foreach (var notifier in AllNotifiers.Values)
            {
                notifier.Update(engineSleeping);
            }
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            if (MainUI.Visible)
            {
                MainUI.Hide();
            }
            else
            {
                MainUI.Show();
                MainUI.RestoreFromMin();
            }
        }

        public override void Stop()
        {
            Settings.Save();
            SoundNotifier[] notifiers = AllNotifiers.Values.ToArray();
            foreach (var notifier in notifiers)
            {
                notifier.Stop(new object(), new EventArgs());
            }
        }

        public void AddNewNotifier()
        {
            string[] allPlayers = WurmClient.WurmPaths.GetAllPlayersNames();
            List<string> availablePlayers = new List<string>();
            foreach (var player in allPlayers)
            {
                if (!AllNotifiers.ContainsKey(player))
                {
                    availablePlayers.Add(player);
                }
            }
            FormChoosePlayer ui = new FormChoosePlayer(availablePlayers.ToArray());
            if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] results = ui.result;
                foreach (var name in results)
                {
                    AddNotifier(name);
                }
            }
        }

        public void AddNotifier(string charName)
        {
            var notifier = new SoundNotifier(this, charName, this.ModuleDataDir);
            MainUI.AddNotifierController(notifier.GetUIHandle());
            AllNotifiers.Add(charName, notifier);
            Settings.Value.ActiveCharacterNames.Add(charName);
            Settings.DelayedSave();
        }

        public void RemoveNotifier(SoundNotifier notifier)
        {
            AllNotifiers.Remove(notifier.Player);
            Settings.Value.ActiveCharacterNames.Remove(notifier.Player);
            Settings.DelayedSave();
            MainUI.RemoveNotifierController(notifier.GetUIHandle());
        }
    }
}
