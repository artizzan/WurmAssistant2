namespace Aldurcraft.Persistent40
{
    public interface IPersistentSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string serializedData);
    }
}
