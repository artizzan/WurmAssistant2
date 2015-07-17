using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    public class PlaylistEntry
    {
        /// <summary>
        /// Name of the sound (file name without extension)
        /// </summary>
        public string SoundName;
        /// <summary>
        /// Soundplayer instance used to cache the sound file
        /// </summary>
        public SB_SoundPlayer Soundplayer;
        /// <summary>
        /// Condition that triggers this sound
        /// </summary>
        public string Condition;
        /// <summary>
        /// List of all special settings
        /// </summary>
        public HashSet<string> SpecialSettings = new HashSet<string>();
        public bool isActive = true;
        public bool isCustomRegex = false;
    }
}
