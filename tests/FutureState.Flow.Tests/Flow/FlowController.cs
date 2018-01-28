using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using FutureState.Flow.BatchControllers;
using FutureState.Flow.Data;

namespace FutureState.Flow.Tests.Flow
{
    using YamlDotNet.Serialization;

    public class FlowController : IDisposable
    {
        /// <summary>
        ///     Gets the number of flow file processed.
        /// </summary>
        public long Processed { get; private set; }

        private readonly IComponentContext _container;

        private readonly FlowConfiguration _config;

        public List<IDisposable> Services { get; }

        public FlowController(FlowConfiguration config, IComponentContext container) //todo: remove container
        {
            Guard.ArgumentNotNull(config, nameof(config));
            Guard.ArgumentNotNull(container, nameof(container));

            _container = container;
            _config = config;

            Services = new List<IDisposable>();
        }

        public static FlowController Load(string file, IComponentContext container)
        {
            return new FlowController(FlowConfiguration.Load(file), container);
        }

        public void Start()
        {
            // initialize directories
            foreach (var definition in _config.Controllers)
            {
                if (!Directory.Exists(definition.InputDirectory))
                    Directory.CreateDirectory(definition.InputDirectory);
                if (!Directory.Exists(definition.OutputDirectory))
                    Directory.CreateDirectory(definition.OutputDirectory);
            }

            foreach (var flowControllerDefinitionse in _config.Controllers.OrderBy(m => m.DateCreated))
                StartController(flowControllerDefinitionse);
        }

        protected void StartController(FlowControllerDefinition definition)
        {
            var batchControllerType = Type.GetType(definition.BatchControllerType);

            // ReSharper disable once UsePatternMatching
            var batchProcessor = _container.Resolve(batchControllerType) as IFlowFileBatchController;
            if (batchProcessor == null)
                throw new InvalidOperationException($"Controller type does not implement {typeof(IFlowFileBatchController).Name}");

            // configure
            batchProcessor.InDirectory = definition.InputDirectory;
            batchProcessor.OutDirectory = definition.OutputDirectory;
            batchProcessor.FlowId = _config.FlowId;
            batchProcessor.ControllerName = definition.ControllerName;

            var logRepository = _container.Resolve<FlowFileLogRepository>();
            logRepository.WorkingFolder = _config.BasePath;

            var processor = _container.Resolve<FlowFileProcessorService>(
                new TypedParameter(typeof(IFlowFileLogRepository), logRepository),
                new TypedParameter(typeof(IFlowFileBatchController), batchProcessor));

            // configure polling internval
            processor.Interval = TimeSpan.FromSeconds(definition.PollInterval);

            // log how many files were processed
            processor.FlowFileProcessed += (o, e) =>
            {
                Processed++;
            };

            Services.Add(processor);

            processor.Start();

        }

        public void Stop()
        {
            foreach (var service in Services)
                service.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}