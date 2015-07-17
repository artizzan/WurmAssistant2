using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Aldurcraft.Persistent40
{
    public class PlainFilePersistentStorage : IPersistentStorage
    {
        private IPersistentLogger Logger { get; set; }
        public string DataStorageId { get; private set; }

        private const string FileExtension = ".pdata";
        private const string FileExtensionNew = ".new";
        private const string FileExtensionOld = ".old";

        private readonly Dictionary<string, Locker> lockBox = new Dictionary<string, Locker>();

        public PlainFilePersistentStorage(IPersistentLogger logger, string persistentStorageId)
        {
            Logger = logger;
            DataStorageId = persistentStorageId;
            if (!Directory.Exists(DataStorageId))
            {
                Directory.CreateDirectory(DataStorageId);
            }
        }

        public void LockObject(string objectId, string objectFullType)
        {
            objectId = objectId.ToLowerInvariant();
            if (objectId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("objectId has invalid filename characters and would not be saveable");
            }
            if (Path.HasExtension(objectId))
            {
                var extension = Path.GetExtension(objectId);
                if (extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(FileExtensionNew, StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(FileExtensionOld, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("objectId cannot have a name that ends with .pdata, .new or .old");
            }
            if (objectId.Length > 200)
            {
                throw new ArgumentException("objectId cannot have more than 200 characters");
            }
            Locker locker;
            if (lockBox.TryGetValue(objectId, out locker))
            {
                if (locker.Locked)
                {
                    throw new InvalidOperationException(
                        "This objectId is already locked by another Persistent, " 
                        + "dispose old instance before creating new one");
                }
                if (!string.IsNullOrEmpty(locker.FullType) && locker.FullType != objectFullType)
                {
                    throw new InvalidOperationException(
                        "This objectId was used with different runtime type during this session, " +
                        "this may indicate a bug");
                }
                locker = locker.Lock();
            }
            else
            {
                locker = new Locker()
                {
                    FullType = objectFullType,
                    Locked = true
                };
            }
            lockBox[objectId] = locker;
        }

        public string GetJsonData(string objectId)
        {
            objectId = objectId + FileExtension;
            string filePath = Path.Combine(DataStorageId, objectId);
            try
            {
                if (!File.Exists(filePath))
                {
                    if (File.Exists(filePath + FileExtensionNew))
                    {
                        File.Move(filePath + FileExtensionNew, filePath);
                    }
                    else if (File.Exists(filePath + FileExtensionOld))
                    {
                        File.Move(filePath + FileExtensionOld, filePath);
                    }
                    else
                    {
                        return null;
                    }
                }
                return RepeatedFileRead(filePath, 5);
            }
            catch (Exception exception)
            {
                Logger.LogInfo("something went wrong with loading object, trying to recover", this, exception);
                try
                {
                    string filePathToBroken = filePath + "_broken_" + DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + ".txt";
                    File.Copy(filePath, filePathToBroken);
                    Logger.LogInfo("created backup of broken settings file for manual data recovery if needed: " + filePathToBroken, this);
                }
                catch (Exception exception1)
                {
                    Logger.LogError("could not create backup of broken settings file: " + filePath, this, exception1);
                }

                // try to recover by assuming current file is corrupted, check if .new or .old still exist
                // if it does, just delete the current file and let the recursive call handle the rest
                if (File.Exists(filePath + ".new"))
                {
                    RepeatedFileDelete(filePath, 5);
                    return GetJsonData(objectId);
                }
                if (File.Exists(filePath + ".old"))
                {
                    RepeatedFileDelete(filePath, 5);
                    return GetJsonData(objectId);
                }
                throw;
            } 
        }

        public void SaveData(string objectId, string jsonData)
        {
            var objectIdPath = objectId + FileExtension;
            string filePath = Path.Combine(DataStorageId, objectIdPath);

            RepeatFileSave(filePath, jsonData, 5);
        }

        public void UnlockObject(string objectId)
        {
            objectId = objectId.ToLowerInvariant();
            lockBox[objectId] = lockBox[objectId].Unlock();
        }

        void RepeatedFileDelete(string filePath, int retryCount = 0)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception exception)
            {
                if (retryCount > 0)
                {
                    Logger.LogInfo("problem deleting file " + filePath + ", retrying in 100ms", this,
                        exception);
                    Thread.Sleep(100);
                    retryCount--;
                    RepeatedFileDelete(filePath, retryCount);
                }
                throw;
            }
        }

        string RepeatedFileRead(string filePath, int retryCount = 0)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    using (var sr = new StreamReader(fileStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception exception)
            {
                if (retryCount > 0)
                {
                    Logger.LogInfo("problem reading file " + filePath + ", retrying in 100ms", this,
                        exception);
                    Thread.Sleep(100);
                    retryCount--;
                    return RepeatedFileRead(filePath, retryCount);
                }
                throw;
            }
        }

        void RepeatFileSave(string filePath, string fileContents, int retryCount = 0)
        {
            try
            {
                if (File.Exists(filePath + ".new")) File.Delete(filePath + ".new");
                if (File.Exists(filePath + ".old")) File.Delete(filePath + ".old");

                File.WriteAllText(filePath + ".new", fileContents, Encoding.UTF8);

                if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");
                File.Move(filePath + ".new", filePath);
                File.Delete(filePath + ".old");
            }
            catch (Exception exception)
            {
                if (retryCount > 0)
                {
                    Logger.LogInfo("problem saving file " + filePath + ", retrying in 100ms", this,
                        exception);
                    Thread.Sleep(100);
                    retryCount--;
                    RepeatFileSave(filePath, fileContents, retryCount);
                }
                throw;
            }
        }

        struct Locker
        {
            public bool Locked;
            public string FullType;

            public Locker Lock()
            {
                return new Locker()
                {
                    FullType = this.FullType,
                    Locked = true
                };
            }

            public Locker Unlock()
            {
                return new Locker()
                {
                    FullType = this.FullType,
                    Locked = false
                };
            }
        }
    }
}
