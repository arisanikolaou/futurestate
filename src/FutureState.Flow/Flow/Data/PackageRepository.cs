using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Data
{
    public class PackageRepository<TData>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncLock = new object();


        public PackageRepository(string basePath)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(basePath, nameof(basePath));

            BasePath = basePath;
        }


        public string BasePath { get; set; }

        public virtual void Save(FlowPackage<TData> flowPackage)
        {
            lock (_syncLock)
            {
                var serializer = new JsonSerializer();

                var filePath = $@"{BasePath}\processor.data.{typeof(TData).Name}.{flowPackage.FlowId}.json";

                if (File.Exists(filePath))
                    File.Delete(filePath); // delete existing files

                using (var file = File.CreateText(filePath))
                {
                    serializer.Serialize(file, flowPackage);
                }
            }
        }

        /// <summary>
        ///     Gets all underling valid entities associated with all discovered packages.
        /// </summary>
        public virtual IEnumerable<TEntity> GetEntities<TEntity>()
        {
            var serializer = new JsonSerializer();

            var packages = Get<TEntity>();

            foreach (var package in packages)
            foreach (var item in package.Data)
                yield return item;
        }

        /// <summary>
        ///     Gets all packages that match a given entity type.
        /// </summary>
        public virtual IEnumerable<FlowPackage<TEntity>> Get<TEntity>()
        {
            var serializer = new JsonSerializer();

            lock (_syncLock)
            {
                foreach (var filePath in Directory.GetFiles(BasePath, $"processor.data.{typeof(TEntity).Name}.*.json"))
                    using (var file = File.OpenText(filePath))
                    {
                        FlowPackage<TEntity> flowPackage = null;

                        try
                        {
                            flowPackage = (FlowPackage<TEntity>) serializer.Deserialize(file, typeof(FlowPackage<TEntity>));

                            if (flowPackage.Data == null)
                                throw new InvalidOperationException(
                                    $"Failed to load flowPackage data from path: {typeof(TEntity).Name}.");
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to read data from file {filePath}.", ex);
                        }

                        yield return flowPackage;
                    }
            }
        }
    }
}