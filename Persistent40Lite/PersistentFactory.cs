namespace Aldurcraft.Persistent40
{
    public class PersistentFactory
    {
        private readonly IPersistentStorage storage;
        private readonly IPersistentSerializer serializer;
        private readonly IPersistentLogger logger;

        private readonly object locker = new object();

        public PersistentFactory(IPersistentStorage storage, IPersistentSerializer serializer, IPersistentLogger logger)
        {
            this.storage = storage;
            this.serializer = serializer;
            this.logger = logger;
        }

        public Persistent<T> Create<T>(string objectId)
            where T : class, new()
        {
            lock (locker)
            {
                var persistent = new Persistent<T>
                {
                    PersistentStorage = storage,
                    PersistentSerializer = serializer,
                    PersistentLogger = logger
                };
                persistent.Initialize(objectId);
                return persistent;
            }
        }
    }
}
