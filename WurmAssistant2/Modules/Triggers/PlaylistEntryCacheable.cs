using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public class PlaylistEntryCacheable
    {
        public SB_SoundPlayer Soundplayer;
        public string Condition;
        public string SoundName;
        public bool isActive = true;

        public PlaylistEntryCacheable(SB_SoundPlayer player, string cond, string SndName, bool Active = true)
        {
            this.Soundplayer = player;
            this.Condition = cond;
            this.SoundName = SndName;
            this.isActive = Active;
        }
    }
}
