using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Aldurcraft.Persistent40
{
    public class JsonPersistentSerializer : IPersistentSerializer
    {
        private IPersistentLogger Logger { get; set; }
        private JsonSerializer Serializer { get; set; }
        private JsonSerializerSettings CustomSettings { get; set; }
        public JsonPersistentSerializer(IPersistentLogger logger)
        {
            Logger = logger;
            CustomSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            if (Serializer == null)
            {
                Serializer = JsonSerializer.Create(CustomSettings);
            }

            Serializer.Error += (sender, args) =>
            {
                args.ErrorContext.Handled = true;
                Logger.LogError("error while deserializing", this, args.ErrorContext.Error);
            };
        }

        public string Serialize(object obj)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                using (var jtw = new JsonTextWriter(sw))
                {
                    Serializer.Serialize(jtw, obj);
                }
            }
            return sb.ToString();
        }

        public T Deserialize<T>(string serializedData)
        {
            using (var sr = new StringReader(serializedData))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    return Serializer.Deserialize<T>(jtr);
                }
            }
        }
    }
}
