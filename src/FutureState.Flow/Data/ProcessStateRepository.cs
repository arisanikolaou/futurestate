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
            string filePath = $@"{BasePath}\processor-state.{_entityType.Name}.json";

            if (File.Exists(filePath))
            {
                lock (_syncLock)
                {
                    var readOutput = File.ReadAllText(filePath);

                    var deserializer = new Deserializer();

                    return deserializer.Deserialize<ProcessState>(readOutput);
                }
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
            lock (_syncLock)
            {
                string filePath = $@"{BasePath}\processor-state.{_entityType.Name}.json";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                var serializer = new Serializer();

                var serialize = serializer.Serialize(state);

                File.WriteAllText(filePath, serialize);
            }
        }
    }
}