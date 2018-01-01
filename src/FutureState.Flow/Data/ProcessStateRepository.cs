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

        /// <summary>
        ///     Loads or creates processor state.
        /// </summary>
        public ProcessState Get()
        {
            string filePath = GetFilePath();

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
                return new ProcessState($"Processor-{_entityType.Name}");
            }
        }

        string GetFilePath() =>  $@"{BasePath}\processor.state.{_entityType.Name}.yaml";

        /// <summary>
        ///     Saves the state.
        /// </summary>
        /// <param name="state">The state to save.</param>
        public void Save(ProcessState state)
        {
            Guard.ArgumentNotNull(state, nameof(state));

            string filePath = GetFilePath();

            var serializer = new Serializer();

            lock (_syncLock)
            {
                string serialize = serializer.Serialize(state);

                // always overwrite
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllText(filePath, serialize);
            }
        }
    }
}