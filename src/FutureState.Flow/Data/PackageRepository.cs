using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
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

                string filePath = $@"{BasePath}\processor.data.{typeof(TData).Name}.{package.FlowId}.json";

                if (File.Exists(filePath))
                    File.Delete(filePath); // delete existing files

                using (var file = File.CreateText(filePath))
                    serializer.Serialize(file, package);
            }
        }

        /// <summary>
        ///     Gets all entities that match a given output entity type.
        /// </summary>
        public virtual IEnumerable<TEntityOut> Get<TEntityOut>()
        {
            var serializer = new JsonSerializer();

            lock (_syncLock)
            {
                foreach (var filePath in Directory.GetFiles(BasePath, $"processor.data.{typeof(TEntityOut).Name}.*.json"))
                {
                    using (var file = File.OpenText(filePath))
                    {
                        Package<TEntityOut> package = null;

                        try
                        {
                            package = (Package<TEntityOut>)serializer.Deserialize(file, typeof(Package<TEntityOut>));

                            if (package.Data == null)
                                throw new InvalidOperationException($"Failed to load package data from path: {typeof(TEntityOut).Name}.");
                        }
                        catch(InvalidOperationException ex)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to read data from file {filePath}.", ex);
                        }

                        foreach (var item in package.Data)
                            yield return item;
                    }
                }

                yield break;
            }
        }
    }
}