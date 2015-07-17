using System;

namespace Aldurcraft.Persistent40
{
    public class Persistent<T> : IDisposable
        where T : class, new()
    {
        internal IPersistentStorage PersistentStorage { get; set; }
        internal IPersistentSerializer PersistentSerializer { get; set; }
        internal IPersistentLogger PersistentLogger { get; set; }
        private T persistedObject;
        private DateTime? scheduledSave = null;
        private string objectFullType;

        private object locker = new object();
        private bool disposed = false;
        private bool initialized = false;

        /// <summary>
        /// Get data object, that is being persisted by this wrapper
        /// </summary>
        public T Data
        {
            get { return persistedObject; }
        }

        internal Persistent()
        {
            // parametrized constructors are not usable in generics, that are intended to be constructed in factories
        }

        public string ObjectId { get; private set; }

        internal void Initialize(string objectId)
        {
            if (objectId == null || string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("objectId cannot be empty");
            }
            ObjectId = objectId;
            objectFullType = typeof(T).AssemblyQualifiedName;
            PersistentStorage.LockObject(objectId, objectFullType);

            try
            {
                bool exists = Load();
                bool reqSave = false;
                if (!exists)
                {
                    if (persistedObject == null)
                    {
                        persistedObject = new T();
                    }
                    reqSave = true;
                }
                var ips = persistedObject as ISynchronizablePersistent;
                if (ips != null)
                {
                    ips.SetSyncRoot(locker);
                }
                if (reqSave)
                {
                    SaveInner();
                }
                initialized = true;
            }
            catch (Exception exception)
            {
                PersistentStorage.UnlockObject(objectId);
                PersistentLogger.LogError("problem initializing Persistent", this, exception);
                throw;
            }
        }

        /// <summary>
        /// Mark this object dirty (requiring save). Save will happen after saveDelay and Update() is called.
        /// </summary>
        /// <param name="saveDelay"></param>
        public void RequireSave(double saveDelay = 5.0D)
        {
            if (scheduledSave == null)
            {
                scheduledSave = DateTime.Now + TimeSpan.FromSeconds(saveDelay);
            }
        }

        /// <summary>
        /// Saves the object if marked dirty.
        /// </summary>
        public void Update()
        {
            if (scheduledSave != null)
            {
                if (DateTime.Now > scheduledSave)
                {
                    scheduledSave = null;
                    Save();
                }
            }
        }

        private void SaveInner()
        {
            var serializedData = PersistentSerializer.Serialize(persistedObject);
            PersistentStorage.SaveData(ObjectId, serializedData);
        }

        /// <summary>
        /// Forces immediate save of the object.
        /// </summary>
        /// <returns></returns>
        public bool Save(bool throwSaveException = false)
        {
            lock (locker)
            {
                if (disposed)
                {
                    throw new Exception("Cannot use this object after disposal");
                }
                if (!initialized)
                {
                    throw new Exception("This object was not initialized properly and cannot be saved");
                }

                try
                {
                    SaveInner();
                    return true;
                }
                catch (Exception exception)
                {
                    PersistentLogger.LogError("something went wrong with saving object", this, exception);
                    if (throwSaveException)
                    {
                        throw;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Forces immediate load of the object, replacing existing object instance if exists.
        /// </summary>
        /// <returns></returns>
        private bool Load()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Cannot use this object after disposal");
            }

            var jsonData = PersistentStorage.GetJsonData(ObjectId);
            if (jsonData != null)
            {
                persistedObject = PersistentSerializer.Deserialize<T>(jsonData);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (!disposed)
                {
                    PersistentStorage.UnlockObject(ObjectId);
                    disposed = true;
                }
            }
        }
    }
}
