using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;
using System.IO;

namespace Aldurcraft.Utility.SoundEngine
{
    /// <summary>
    /// Wraps around single sound handle and provides some simple methods to work with it
    /// </summary>
    public class SB_SoundPlayer : IDisposable
    {
        string filePath;
        ISoundSource soundRef;
        bool isInitalized = false;
        private string _soundName;

        public string SoundName
        {
            get { return _soundName; }
            private set { _soundName = value; }
        }

        /// <summary>
        /// Create a new wrapper for sound file located at specified path
        /// </summary>
        /// <param name="filePath">recommended absolute path</param>
        public SB_SoundPlayer(string filePath)
        {
            this.filePath = filePath;
            this.SoundName = Path.GetFileName(filePath);
        }

        public SB_SoundPlayer()
        {
            this.SoundName = string.Empty;
        }

        /// <summary>
        /// Initializes the sound
        /// </summary>
        /// <param name="defVolume">default volume for the sound, between 0.0F and 1.0F</param>
        /// <param name="volumeAdjust">do not attempt at all to set volume for this sound (used as override for irrklang issue)</param>
        public void Load(float defVolume = 1.0F, bool volumeAdjust = true)
        {
            soundRef = SoundBank.SoundEngine.AddSoundSourceFromFile(filePath);
            isInitalized = true;
            if (volumeAdjust) ChangeVolume(defVolume);
        }

        /// <summary>
        /// Plays the sound
        /// </summary>
        public bool Play()
        {
            if (isInitalized)
            {
                // check if the sound exists in engine
                var result = SoundBank.SoundEngine.Play2D(filePath);
                if (result == null) return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adjusts volume for this sound
        /// </summary>
        /// <param name="volume"></param>
        public void ChangeVolume(float volume)
        {
            if (isInitalized)
            {
                volume = GeneralHelper.ConstrainValue(volume, 0.0F, 1.0F);
                soundRef.DefaultVolume = volume;
            }
        }

        /// <summary>
        /// Removes the sound freeing unmanaged resources, wrapper for dispose
        /// </summary>
        public void Remove()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (soundRef != null) soundRef.Dispose();
        }
    }
}
