using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.BatchControllers;
using YamlDotNet.Serialization;

namespace FutureState.Flow
{
    public class FlowConfiguration
    {
        /// <summary>
        ///     Gets the flow's unique identifier.
        /// </summary>
        public Guid FlowId { get; set; }
        /// <summary>
        ///     Gets the working directory for the flow.
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        ///     Gets the controller to start/stop.
        /// </summary>
        public List<FlowControllerDefinition> Controllers { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowConfiguration()
        {

        }

        public FlowConfiguration(Guid flowId)
        {
            FlowId = flowId;
            BasePath = Environment.CurrentDirectory;
            Controllers = new List<FlowControllerDefinition>();
        }

        public FlowControllerDefinition AddController<T>(string controllerName)
            where T : IFlowFileBatchController
        {
            FlowControllerDefinition def;

            if (Controllers == null)
                Controllers = new List<FlowControllerDefinition>();

            var lastOutputDirectory = Controllers.LastOrDefault()?.OutputDirectory;

            Controllers.Add(def = new FlowControllerDefinition()
            {
                ControllerName = controllerName,
                BatchControllerType = typeof(T).AssemblyQualifiedName,
                // the last output directory will be the input to this one
                InputDirectory = lastOutputDirectory ?? $@"{BasePath}\{controllerName}\In",
                OutputDirectory = $@"{BasePath}\{controllerName}\Out",
                PollInterval = 2, // seconds
                DateCreated = DateTime.UtcNow
            });

            return def;
        }

        public void Save(string fileName = null)
        {
            fileName = fileName ?? "flow-config.yaml";

            // save yaml config
            var serializer = new Serializer();
            string yamlFile = $@"{BasePath}\{fileName}";
            if (File.Exists(yamlFile))
                File.Delete(yamlFile);

            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            using (var fs = new StreamWriter(File.Create(yamlFile)))
            {
                serializer.Serialize(fs, this);
                fs.Flush();
            }
        }

        public static FlowConfiguration Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"File {filePath} does not exist.");

            var deserializer = new Deserializer();
            using (var fs = new StreamReader(filePath))
                return deserializer.Deserialize<FlowConfiguration>(fs);
        }
    }

    public class FlowControllerDefinition
    {
        /// <summary>
        ///     Gets the frequency, in secondss, to poll for new data files.
        /// </summary>
        public int PollInterval { get; set; }
        /// <summary>
        ///     Gets the assembly qualified name of the batch controller to use to process the data.
        /// </summary>
        public string BatchControllerType { get; set; }
        /// <summary>
        ///     Gets the display name of the controller.
        /// </summary>
        public string ControllerName { get; set; }
        /// <summary>
        ///     Gets the port source for data.
        /// </summary>
        public string InputDirectory { get; set; }
        /// <summary>
        ///     Gets the output path for processed data.
        /// </summary>
        public string OutputDirectory { get; set; }
        /// <summary>
        ///     Gets the date the entry was recorded.
        /// </summary>
        public DateTime DateCreated { get; set; }
    }
}
