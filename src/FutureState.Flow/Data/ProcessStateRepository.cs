using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace FutureState.Flow.Data
{
    public class ProcessStateRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncLock = new object();
        private readonly Type _entityType;

        public ProcessStateRepository(string basePath, Type entityType)
        {
            BasePath = basePath;
            _entityType = entityType;
        }

        public string BasePath { get; set; }

        public ProcessState Get()
        {
            string filePath = $@"{BasePath}\processor.state.{_entityType.Name}.yaml";

            if (File.Exists(filePath))
            {
                var deserializer = new Deserializer();

                string text = null;
                lock (_syncLock)
                    text = File.ReadAllText(filePath);

                return deserializer.Deserialize<ProcessState>(text);
            }
            else
            {
                return new ProcessState()
                {
                    Details = new List<ProcessFlowState>()
                };
            }
        }

        public void Save(ProcessState state)
        {
            Guard.ArgumentNotNull(state, nameof(state));

            string filePath = $@"{BasePath}\processor.state.{_entityType.Name}.yaml";

            var serializer = new Serializer();

            lock (_syncLock)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                string serialize = serializer.Serialize(state);

                File.WriteAllText(filePath, serialize);
            }
        }
    }
}