using System.IO;
using NLog;
using YamlDotNet.Serialization;

namespace FutureState.Flow.Data
{
    public class ProcessConfigurationRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _configFilePath;
        private readonly object _syncLock = new object();

        public ProcessConfigurationRepository(string configFilePath)
        {
            _configFilePath = configFilePath;
        }

        /// <summary>
        ///     Load processor configuration.
        /// </summary>
        public virtual ProcessorConfiguration Get()
        {
            if (File.Exists(_configFilePath))
                lock (_syncLock)
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace($"Loading processor configuration from file {_configFilePath}.");

                    var readOutput = File.ReadAllText(_configFilePath);

                    var deserializer = new Deserializer();

                    return deserializer.Deserialize<ProcessorConfiguration>(readOutput);
                }

            return new ProcessorConfiguration("Default");
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