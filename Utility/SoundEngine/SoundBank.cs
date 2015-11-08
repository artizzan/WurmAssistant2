using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using IrrKlang;
using System.Data;

namespace Aldurcraft.Utility.SoundEngine
{
    /// <summary>
    /// Wrapper around IrrKlang sound engine, supports adding, managing and playing sounds, adjusting volumes.
    /// Configuration GUI available via WinForms dialog window.
    /// </summary>
    static public class SoundBank
    {
        const string THIS = "SoundBank";

        internal static ISoundEngine SoundEngine;
        internal static string SoundsDirectory;

        static Dictionary<string, SB_SoundPlayer> dictSoundBank = new Dictionary<string, SB_SoundPlayer>();
        static List<string> allSoundsNames = new List<string>();

        static Dictionary<string, float> AdjustedVolumesDict = new Dictionary<string, float>();
        static DataSet AdjustedVolumesStorage = new DataSet("DefVolSaved");

        internal static float globalvolume = 1.0F;

        /// <summary>
        /// Global volume for all sounds, between 0.0F and 1.0F
        /// </summary>
        public static float GlobalVolume
        {
            get { return globalvolume; }
            set
            {
                globalvolume = GeneralHelper.ConstrainValue(value, 0.0F, 1.0F);
                SoundEngine.SoundVolume = globalvolume;
            }
        }

        /// <summary>
        /// Set global volume for all sounds, between 0.0F and 1.0F
        /// </summary>
        [Obsolete] 
        public static void ChangeGlobalVolume(float volume)
        {
            GlobalVolume = volume;
        }

        /// <summary>
        /// Initialize SoundBank with optional dataDir where settings and sounds should be stored.
        /// If no dataDir is specified, uses default [CodeBase]\SoundBank\
        /// </summary>
        /// <param name="dataDir">full directory path</param>
        /// <exception cref="ArgumentException">Directory path was probably invalid</exception>
        public static void InitializeSoundBank(string dataDir = null)
        {
            if (dataDir != null) SoundsDirectory = dataDir;
            else SoundsDirectory = GeneralHelper.PathCombineWithCodeBasePath("SoundBank");
            CreateSoundBank();
        }

        static bool SoundBankCreated = false;

        internal static void CreateSoundBank()
        {
            Logger.LogInfo("Initializing", THIS);
            SoundEngine = new ISoundEngine();
            SoundBankCreated = true;
            if (!Directory.Exists(SoundsDirectory))
            {
                Directory.CreateDirectory(SoundsDirectory);
            }
            InitDefVolumesDict();
            BuildSoundBank();
            Logger.LogInfo("Init completed", THIS);
        }

        /// <summary>
        /// Rebuild and reinitialize all sounds in SoundBank
        /// </summary>
        public static void RebuildSoundBank()
        {
            BuildSoundBank();
        }

        internal static void BuildSoundBank()
        {
            if (SoundBankCreated)
            {
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.wav"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.mp3"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.ogg"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.flac"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.mod"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.it"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.s3d"));
                files.AddRange(Directory.GetFiles(SoundsDirectory, "*.xm"));

                SoundEngine.RemoveAllSoundSources();
                dictSoundBank.Clear();
                allSoundsNames.Clear();
                foreach (string file in files)
                {
                    SB_SoundPlayer newsound = new SB_SoundPlayer(file);
                    float volume;
                    if (AdjustedVolumesDict.TryGetValue(Path.GetFileName(file), out volume))
                    {
                        newsound.Load(volume);
                    }
                    else newsound.Load();
                    dictSoundBank.Add(Path.GetFileName(file), newsound);
                    allSoundsNames.Add(Path.GetFileName(file));
                }
            }
            else
            {
                Logger.LogCritical("!!! Init before Create, no sounds loaded", THIS);
            }
        }

        /// <summary>
        /// Plays sound if exists, else nothing happens
        /// </summary>
        /// <param name="name">Name of the sound, case sensitive, no file extension</param>
        public static void PlaySound(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.LogDiag("NullOrEmpty sound name at SoundBank.PlaySound", THIS);
                return;
            }

            SB_SoundPlayer player;
            if (dictSoundBank.TryGetValue(name, out player))
            {
                try { player.Play(); }
                catch (FileNotFoundException _e)
                {
                    BuildSoundBank();
                    Logger.LogError("sound file was missing: " + (name ?? "NULL"), THIS, _e);
                }
            }
        }

        /// <summary>
        /// Returns SoundPlayer instance for specified sound or null if not exists
        /// </summary>
        /// <param name="soundname">Name of the sound, case sensitive, no file extension</param>
        public static SB_SoundPlayer GetSoundPlayer(string soundname)
        {
            SB_SoundPlayer player;
            if (dictSoundBank.TryGetValue(soundname, out player))
            {
                return player;
            }
            else return null;
        }

        public static SB_SoundPlayer GetSoundPlayerNoNulls(string soundname)
        {
            return GetSoundPlayer(soundname) ?? new SB_SoundPlayer();
        }

        /// <summary>
        /// Returns names of all cached sounds
        /// </summary>
        /// <returns></returns>
        public static string[] GetSoundsArray()
        {
            return allSoundsNames.ToArray();
        }

        /// <summary>
        /// Immediatelly stops all currectly playing sounds
        /// </summary>
        public static void StopSounds()
        {
            try
            {
                SoundEngine.StopAllSounds();
            }
            catch (Exception _e)
            {
                Logger.LogDebug("Could not stop sounds", THIS, _e);
            }
        }

        /// <summary>
        /// Returns volume level for this sound
        /// </summary>
        /// <param name="soundname">Case sensitive, no file extension</param>
        /// <returns></returns>
        internal static float GetVolumeForSound(string soundname)
        {
            float adjvolume;
            if (AdjustedVolumesDict.TryGetValue(soundname, out adjvolume))
            {
                return adjvolume;
            }
            else return 1.0F;
        }

        /// <summary>
        /// Adjusts volume for this sound
        /// </summary>
        /// <param name="soundname">Case sensitive, no file extension</param>
        /// <param name="volume">Between 0.0F and 1.0F</param>
        internal static void AdjustVolumeForSound(string soundname, float volume)
        {
            AdjustedVolumesDict[soundname] = GeneralHelper.ConstrainValue(volume, 0.0F, 1.0F);
            SB_SoundPlayer player;
            if (dictSoundBank.TryGetValue(soundname, out player))
            {
                player.ChangeVolume(volume);
            }
            SaveAdjustedVolumeStorage();
        }

        internal static void RemoveSound(string soundname)
        {
            try
            {
                File.Delete(SoundsDirectory + @"\" + soundname);
            }
            catch (Exception _e)
            {
                RemoveAdjustedVolumeEntryFromDict(soundname);
                Logger.LogError("Error while trying to delete sound: " + (soundname ?? "NULL"), THIS, _e);
            }
            BuildSoundBank();
        }

        static void RemoveAdjustedVolumeEntryFromDict(string sndname)
        {
            try //debugging
            { AdjustedVolumesDict.Remove(Path.GetFileName(sndname)); }
            catch (Exception _e) { Logger.LogError("", THIS, _e); }
        }

        internal static void RenameSound(string oldpath, string newpath)
        {
            bool moveFailed = false;
            try
            {
                File.Move(oldpath, newpath);
            }
            catch (Exception _e)
            {
                moveFailed = true;
                Logger.LogError("Exception while renaming file " + oldpath + " to " + newpath, THIS, _e);
            }
            if (!moveFailed)
            {
                float oldAdjVolume = GetVolumeForSound(Path.GetFileName(oldpath));
                RemoveAdjustedVolumeEntryFromDict(Path.GetFileName(oldpath));
                AdjustVolumeForSound(Path.GetFileName(newpath), oldAdjVolume);
            }
            BuildSoundBank();
        }

        internal static void AddSound(string filename)
        {
            try
            {
                File.Copy(filename, SoundsDirectory + @"\" + Path.GetFileName(filename));
            }
            catch (IOException _e)
            {
                if (MessageBox.Show(Path.GetFileName(filename) + " already exists in SoundBank, add it anyway?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        bool fileCopied = false;
                        int index = 1;
                        while (!fileCopied)
                        {
                            string newSavePath = SoundsDirectory + @"\" + Path.GetFileNameWithoutExtension(filename) + "_" + index + Path.GetExtension(filename);
                            if (!File.Exists(newSavePath))
                            {
                                File.Copy(filename, newSavePath);
                                fileCopied = true;
                            }
                            else index++;
                        }
                    }
                    catch
                    {
                        Logger.LogError("Exception while trying to copy " + filename, THIS, _e);
                    }
                }
            }
        }

        /// <summary>
        /// Open SoundBank GUI
        /// </summary>
        /// <returns></returns>
        public static bool OpenSoundBank()
        {
            FormSoundBank SoundBankUI = new FormSoundBank();
            if (SoundBankUI.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Shows a dialog asking to choose a single sound from SoundBank.
        /// If no sound is chosen, returns null, else returns sound name.
        /// </summary>
        /// <returns></returns>
        public static string ChooseSound()
        {
            FormChooseSound ChooseSoundUI = new FormChooseSound();
            if (ChooseSoundUI.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return ChooseSoundUI.ChosenSound;
            }
            else return null;
        }

        #region ADJUSTED VOLUMES XML STORAGE

        internal static void InitDefVolumesDict()
        {
            bool readFailed = false;
            try { AdjustedVolumesStorage.ReadXml(Path.Combine(SoundsDirectory, "AdjVolSaved.xml")); }
            catch (DirectoryNotFoundException)
            {
                readFailed = true;
                Logger.LogCritical("Note: could not load Sound Bank adjusted volumes due directory missing", THIS);
            }
            catch (FileNotFoundException)
            {
                readFailed = true;
                Logger.LogDebug("Note: could not load Sound Bank adjusted volumes due file missing", THIS);
            }
            catch (Exception _e)
            {
                readFailed = true;
                Logger.LogError("!! Error while loading Sound Bank adjusted volumes", THIS, _e);
            }
            if (!readFailed)
            {
                try // debug
                {
                    foreach (DataRow row in AdjustedVolumesStorage.Tables[0].Rows)
                    {
                        AdjustedVolumesDict.Add(Convert.ToString(row[0]), (float)(Convert.ToDouble(row[1])));
                    }
                }
                catch (Exception _e)
                {
                    Logger.LogError("!! Error while populating Sound Bank adjusted volumes dict", THIS, _e);
                }
            }
        }

        internal static void SaveAdjustedVolumeStorage()
        {
            AdjustedVolumesStorage.Clear();
            AdjustedVolumesStorage.Tables.Add();

            AdjustedVolumesStorage.Tables[0].Columns.Add();
            AdjustedVolumesStorage.Tables[0].Columns.Add();

            foreach (var keyvalue in AdjustedVolumesDict)
            {
                string[] data = new string[2];
                data[0] = keyvalue.Key;
                data[1] = keyvalue.Value.ToString();
                try
                {
                    AdjustedVolumesStorage.Tables[0].Rows.Add(data);
                }
                catch (Exception _e)
                {
                    Logger.LogError("", THIS, _e);
                }
            }
            try
            {
                AdjustedVolumesStorage.WriteXml(SoundsDirectory + @"\AdjVolSaved.xml");
            }
            catch (Exception _e)
            {
                Logger.LogError("!! Error while saving Sound Bank adjusted volumes", THIS, _e);
            }
        }

        #endregion

        public static SoundData TryGetSoundData(string soundName)
        {
            SB_SoundPlayer player;
            if (dictSoundBank.TryGetValue(soundName, out player))
            {
                var data = player.TryGetSoundData();
                data.SoundFileName = data.SoundFileName;
                return data;
            }
            else return null;
        }
    }
}