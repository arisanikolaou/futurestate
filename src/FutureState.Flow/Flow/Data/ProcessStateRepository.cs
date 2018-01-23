using System;
using System.IO;
using NLog;
using YamlDotNet.Serialization;

namespace FutureState.Flow.Data
{
    public class ProcessStateRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Type _entityType;
        private readonly object _syncLock = new object();

        public ProcessStateRepository(string basePath, Type entityType)
        {
            BasePath = basePath;
            _entityType = entityType;
        }

        public string BasePath { get; set; }

        /// <summary>
        ///     Loads or creates processor state.
        /// </summary>
        public FlowProcessState Get()
        {
            var filePath = GetFilePath();

            if (File.Exists(filePath))
            {
                var deserializer = new Deserializer();

                string text = null;
                lock (_syncLock)
                {
                    text = File.ReadAllText(filePath);
                }

                return deserializer.Deserialize<FlowProcessState>(text);
            }

            return new FlowProcessState($"FlowProcessor-{_entityType.Name}");
        }

        private string GetFilePath()
        {
            return $@"{BasePath}\processor.state.{_entityType.Name}.yaml";
        }

        /// <summary>
        ///     Saves the state.
        /// </summary>
        /// <param name="state">The state to save.</param>
        public void Save(FlowProcessState state)
        {
            Guard.ArgumentNotNull(state, nameof(state));

            var filePath = GetFilePath();

            var serializer = new Serializer();

            lock (_syncLock)
            {
                var serialize = serializer.Serialize(state);

                // always overwrite
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllText(filePath, serialize);
            }
        }
    }
}