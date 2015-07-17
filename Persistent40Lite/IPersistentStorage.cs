namespace Aldurcraft.Persistent40
{
    public interface IPersistentStorage
    {
        string DataStorageId { get; }
        void LockObject(string objectId, string objectFullType);
        string GetJsonData(string objectId);
        void SaveData(string objectId, string jsonData);
        void UnlockObject(string objectId);
    }
}
