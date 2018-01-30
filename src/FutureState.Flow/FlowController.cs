using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.BatchControllers;
using FutureState.Flow.Data;

namespace FutureState.Flow.Tests.Flow
{
    using YamlDotNet.Serialization;

    public interface IFlowFileBatchControllerFactory
    {
        IFlowFileBatchController Create(Type type);
    }

    public interface IFlowFileLogRepositoryFactory
    {
        FlowFileLogRepository Get();
    }

    public interface IFlowFileControllerServiceFactory
    {
        FlowFileControllerService Get(IFlowFileLogRepository repository, IFlowFileBatchController controller);
    }

    public class FlowController : IDisposable
    {
        /// <summary>
        ///     Gets the number of flow file processed.
        /// </summary>
        public long Processed { get; private set; }

        private FlowConfiguration _config;
        private readonly IFlowFileBatchControllerFactory _flowControllerFactory;
        private readonly IFlowFileLogRepositoryFactory _flowFileLogFactory;
        private readonly IFlowFileControllerServiceFactory _flowControllerServiceFactory;

        public List<IDisposable> Services { get; }

        public FlowController(
            IFlowFileBatchControllerFactory flowControllerFactory,
            IFlowFileLogRepositoryFactory flowFileLogFactory,
            IFlowFileControllerServiceFactory flowControllerServiceFactory) //todo: remove container
        {
            _flowControllerFactory = flowControllerFactory;
            _flowFileLogFactory = flowFileLogFactory;
            _flowControllerServiceFactory = flowControllerServiceFactory;

            Services = new List<IDisposable>();
        }


        public void Start(FlowConfiguration config)
        {
            _config = config;

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
            var batchProcessor = _flowControllerFactory.Create(batchControllerType);
            if (batchProcessor == null)
                throw new InvalidOperationException($"Controller type does not implement {typeof(IFlowFileBatchController).Name}");

            // configure
            batchProcessor.InDirectory = definition.InputDirectory;
            batchProcessor.OutDirectory = definition.OutputDirectory;
            batchProcessor.FlowId = _config.FlowId;
            batchProcessor.ControllerName = definition.ControllerName;

            var logRepository = _flowFileLogFactory.Get();
            logRepository.WorkingFolder = _config.BasePath;

            FlowFileControllerService processor = _flowControllerServiceFactory.Get(logRepository, batchProcessor);

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