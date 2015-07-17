using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;
using System.Runtime.Serialization;
using System.IO;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Backpack
{
    public class ModuleBackpack : AssistantModule
    {
        [DataContract]
        public class BackpackSettings
        {
            [DataMember]
            public System.Drawing.Point SavedWindowSize = new System.Drawing.Point();

            public BackpackSettings()
            {
                InitMe();
            }

            [OnDeserializing]
            void OnDes(StreamingContext context)
            {
                InitMe();
            }

            void InitMe()
            {
                SavedWindowSize = new System.Drawing.Point(622, 248);
            }
        }

        internal PersistentObject<BackpackSettings> Settings;

        public override void Initialize()
        {
            base.Initialize();

            Settings = new PersistentObject<BackpackSettings>(new BackpackSettings());
            Settings.SetFilePathAndLoad(Path.Combine(this.ModuleDataDir, "settings.xml"));
        }

        FormBackpack gui;

        public override void OpenUI(object sender, EventArgs e)
        {
            try
            {
                gui.Show();
                if (gui.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    gui.WindowState = System.Windows.Forms.FormWindowState.Normal;
                gui.BringToFront();
            }
            catch (Exception)
            {
                gui = new FormBackpack(this);
                gui.Show();
            }
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
        }

        public override void Stop()
        {
            Settings.Save();
            if (gui != null) gui.Close();
            base.Stop();
        }
    }
}
