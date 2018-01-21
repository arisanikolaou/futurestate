using FutureState.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace FutureState.Flow.Data
{
    public class QueryResponseStateRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly object _syncLock = new object();

        public string BasePath { get; set; }

        private readonly Type _entityType;

        public QueryResponseStateRepository(string basePath, Type entityType)
        {
            BasePath = basePath;

            _entityType = entityType;
        }

        public List<QueryResponseState> Get(string processorId)
        {
            string filePath = $@"{BasePath}\processor.source.{_entityType.Name}.{processorId}.yaml";

            if (File.Exists(filePath))
            {
                string readOutput = null;
                lock (_syncLock)
                    readOutput = File.ReadAllText(filePath);

                var deserializer = new Deserializer();

                return deserializer.Deserialize<List<QueryResponseState>>(readOutput);
            }
            else
            {
                return new List<QueryResponseState>();
            }
        }

        public void Save(string processorId, List<QueryResponseState> state)
        {
            string filePath = $@"{BasePath}\processor.source.{_entityType.Name}.{processorId}.yaml";

            if (_logger.IsTraceEnabled)
                _logger.Trace($"Saving query response state to file {filePath}.");

            lock (_syncLock)
            {
                var serializer = new Serializer();

                var serialize = serializer.Serialize(state);

                File.WriteAllText(filePath, serialize);
            }
        }
    }
}