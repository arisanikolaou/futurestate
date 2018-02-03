using FutureState.Flow.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance using a given flow id.
        /// </summary>
        public FlowConfiguration(Guid flowId)
        {
            FlowId = flowId;
            BasePath = Environment.CurrentDirectory;
            Controllers = new List<FlowControllerDefinition>();
        }

        public FlowControllerDefinition AddController<T>(string controllerName)
            where T : IFlowFileController
        {
            FlowControllerDefinition def;

            if (Controllers == null)
                Controllers = new List<FlowControllerDefinition>();

            var lastOutputDirectory = Controllers.LastOrDefault()?.Output;

            Controllers.Add(def = new FlowControllerDefinition()
            {
                ControllerName = controllerName,
                TypeName = typeof(T).AssemblyQualifiedName,
                // the last output directory will be the input to this one
                Input = lastOutputDirectory ?? $@"{BasePath}\{controllerName}\In",
                Output = $@"{BasePath}\{controllerName}\Out",
                PollInterval = 2, // seconds
                ExecutionOrder = Controllers.Count
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
        ///     Gets the assembly qualified name of the batch controller to use to process the data.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets the display name of the controller.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///     Gets the port source for data.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        ///     Gets the output path for processed data.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        ///     Gets the frequency, in secondss, to poll for new data files.
        /// </summary>
        public int PollInterval { get; set; }

        /// <summary>
        ///     Gets the list of validation rules to apply to outgoing entities.
        /// </summary>
        public List<ValidationRule> FieldValidationRules { get; set; } = new List<ValidationRule>();

        /// <summary>
        ///     Flow controller configuration details.
        /// </summary>
        public Dictionary<string, string> ConfigurationDetails { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Gets the relative execution order to the associated controller.
        /// </summary>
        public int ExecutionOrder { get; set; }
    }

    /// <summary>
    ///     Defines the rules to use to validate a given entity.
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        ///     Gets the name of the field to validate.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        ///     Gets the regular expression to run.
        /// </summary>
        public string RegEx { get; set; }

        /// <summary>
        ///     Gets the error message to display.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}