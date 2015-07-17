namespace Aldurcraft.Persistent40
{
    public interface ISynchronizablePersistent
    {
        void SetSyncRoot(object locker);
    }
}
