using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// This class wraps a single object of a [DataContract] enabled class,
    /// allowing to save and load it's state.
    /// Thread safe.
    /// </summary>
    /// <typeparam name="T">Type of the [DataContract] enabled class</typeparam>
    public class PersistentObject<T>
    {
        private T persistentObject;

        private object _locker = new object();

        public T Value
        {
            get { return persistentObject; }
        }

        /// <summary>
        /// Construct wrapper for object.
        /// </summary>
        /// <param name="wrappedObject">must be [DataContract] enabled class ([Serializable] not recommended)</param>
        public PersistentObject(T wrappedObject)
        {
            lock (_locker)
            {
                persistentObject = wrappedObject;
            }
        }

        DateTime? saveTime = null;

        /// <summary>
        /// Will persist the object after specified number of seconds (default: 5).
        /// Must be Update()d before saving can occour.
        /// </summary>
        /// <param name="seconds">default 5 seconds</param>
        public void DelayedSave(double seconds = 5.0D)
        {
            // this is tsafe because it's supposed only to prevent endless saveTime overwriting,
            // delaying save potentially indefinitely
            if (saveTime != null) saveTime = DateTime.Now + TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Attempts to save object, if DelayedSave() was scheduled earlier.
        /// </summary>
        /// <exception cref="InvalidOperationException">filepath not set</exception>
        public void Update()
        {
            // this is tsafe because at worst it will call Save 2 times, which will hit lock at Save
            if (saveTime != null)
            {
                if (DateTime.Now > saveTime)
                {
                    saveTime = null;
                    this.Save();
                }
            }
        }

        bool directoryVerified = false;

        /// <summary>
        /// Save the object, here and now!
        /// </summary>
        /// <returns>true if success</returns>
        /// <exception cref="InvalidOperationException">filepath not set</exception>
        public bool Save()
        {
            lock (_locker)
            {
                if (FilePath == null) ThrowException();

                try
                {
                    VerifyDirectory();
                    if (File.Exists(FilePath + ".new")) File.Delete(FilePath + ".new");
                    if (File.Exists(FilePath + ".old")) File.Delete(FilePath + ".old");

                    var ns = new NetDataContractSerializer();
                    XmlWriterSettings settings = new XmlWriterSettings() {Indent = true};
                    using (XmlWriter xwriter = XmlWriter.Create(FilePath + ".new", settings))
                    {
                        ns.WriteObject(xwriter, persistentObject);
                    }

                    if (File.Exists(FilePath)) File.Move(FilePath, FilePath + ".old");
                    File.Move(FilePath + ".new", FilePath);
                    File.Delete(FilePath + ".old");
                    this.saveTime = null;
                    return true;
                }
                catch (System.IO.FileLoadException exception)
                {
                    if (exception.Message.Contains("System.Core, Version=2.0.5.0"))
                    {
                        Logger.LogError("Detected known assembly resolve error. If NET Framework 4.0 is the installed framework version, "
                            + "install this update for Windows XP: http://support.microsoft.com/kb/2468871 "
                            + "or upgrade to framework 4.5 if running Windows Vista or 7");
                        return false;
                    }
                    else
                    {
                        Logger.LogError("something went wrong with saving object", this, exception);
                        return false;
                    }
                }
                catch (Exception _e)
                {
                    Logger.LogError("something went wrong with saving object", this, _e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Analogous to:
        /// Settings.FilePath = filePath;
        /// if (!Settings.Load()) Settings.Save();
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFilePathAndLoad(string filePath)
        {
            FilePath = filePath;
            if (!Load()) 
                Save();
        }

        void VerifyDirectory()
        {
            if (!directoryVerified)
            {
                if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                directoryVerified = true;
            }
        }

        /// <summary>
        /// Load the object from persisted file, if exists. Else will use default.
        /// </summary>
        /// <returns>true if success</returns>
        /// <exception cref="InvalidOperationException">filepath not set</exception>
        public bool Load()
        {
            lock (_locker)
            {
                if (FilePath == null) ThrowException();

                try
                {
                    VerifyDirectory();
                    if (!File.Exists(FilePath))
                    {
                        if (File.Exists(FilePath + ".new"))
                        {
                            File.Move(FilePath + ".new", FilePath);
                        }
                        else if (File.Exists(FilePath + ".old"))
                        {
                            File.Move(FilePath + ".old", FilePath);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    var ns = new NetDataContractSerializer();
                    using (Stream s = File.OpenRead(FilePath))
                    {
                        persistentObject = (T)ns.ReadObject(s);
                    }
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogInfo("something went wrong with loading object, trying to recover", this, _e);
                    try
                    {
                        string FilePath_toBroken = FilePath + "_broken_" + TimeHelper.GetStringForDateNow() + ".txt";
                        File.Copy(FilePath, FilePath_toBroken);
                        Logger.LogInfo("created backup of broken settings file for manual data recovery if needed: " + FilePath_toBroken, this);
                    }
                    catch (Exception _ee)
                    {
                        Logger.LogError("could not create backup of broken settings file: " + FilePath, this, _ee);
                    }

                    // try to recover by assuming current file is corrupted, check if .new or .old still exist
                    // if it does, just delete the current file and let the recursive call handle the rest
                    if (File.Exists(FilePath + ".new"))
                    {
                        File.Delete(FilePath);
                        return Load();
                    }
                    else if (File.Exists(FilePath + ".old"))
                    {
                        File.Delete(FilePath);
                        return Load();
                    }
                    else
                    {
                        Logger.LogInfo("could not find anything to recover from, is this first launch?", this);
                        return false;
                    }
                } 
            }
        }

        void ThrowException()
        {
            throw new InvalidOperationException("No FilePath was set");
        }

        private string _FilePath = null;

        /// <summary>
        /// Absolute file path to where this file should be persisted, eg "C:\MyCoolProg\settings.xml"
        /// </summary>
        public string FilePath
        {
            get { return _FilePath; }
            set { _FilePath = value; }
        }
    }
}
