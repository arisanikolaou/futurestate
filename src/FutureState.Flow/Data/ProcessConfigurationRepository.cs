using NLog;
using System.IO;
using YamlDotNet.Serialization;

namespace FutureState.Flow.Data
{
    public class ProcessConfigurationRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncLock = new object();
        private readonly string _configFilePath;

        public ProcessConfigurationRepository(string filePath)
        {
            _configFilePath = filePath;
        }

        /// <summary>
        ///     Load processor configuration.
        /// </summary>
        public virtual ProcessorConfiguration Get()
        {
            if (File.Exists(_configFilePath))
            {
                lock (_syncLock)
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace($"Loading processor configuration from file {_configFilePath}.");

                    var readOutput = File.ReadAllText(_configFilePath);

                    var deserializer = new Deserializer();

                    return deserializer.Deserialize<ProcessorConfiguration>(readOutput);
                }
            }

            return new ProcessorConfiguration();
        }

        /// <summary>
        ///     Save processor configuration.
        /// </summary>
        public void Save(ProcessorConfiguration configuration)
        {
            var serializer = new Serializer();

            if (_logger.IsTraceEnabled)
                _logger.Trace($"Saving processor configuration to file {_configFilePath}.");

            lock (_syncLock)
            {
                var serialize = serializer.Serialize(configuration);

                File.WriteAllText(_configFilePath, serialize);
            }
        }
    }
}