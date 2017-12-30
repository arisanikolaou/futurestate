using Newtonsoft.Json;
using NLog;
using System.IO;

namespace FutureState.Flow.Data
{
    public class PackageRepository<TData>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncLock = new object();

        public string BasePath { get; set; }

        public PackageRepository(string basePath)
        {
            BasePath = basePath;
        }

        public virtual void Save(Package<TData> package)
        {
            lock (_syncLock)
            {
                var serializer = new JsonSerializer();

                string filePath = $@"{BasePath}\data.{typeof(TData).Name}.{package.FlowId}.json";

                if (File.Exists(filePath))
                    File.Delete(filePath); // delete existing files

                using (var file = File.CreateText(filePath))
                    serializer.Serialize(file, package);
            }
        }
    }
}