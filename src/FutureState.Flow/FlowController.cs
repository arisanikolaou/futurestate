using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Controllers;
using FutureState.Flow.Data;

namespace FutureState.Flow
{
    using FutureState.Specifications;

    public interface IFlowFileControllerFactory
    {
        IFlowFileController Create(Type type);
    }

    public interface IFlowFileLogRepositoryFactory
    {
        FlowFileLogRepository Get();
    }

    public interface IFlowFileControllerServiceFactory
    {
        FlowFileControllerService Get(IFlowFileLogRepository repository, IFlowFileController controller);
    }

    public class FlowController : IDisposable
    {
        /// <summary>
        ///     Gets the number of flow file processed.
        /// </summary>
        public long Processed { get; private set; }

        private FlowConfiguration _config;
        private readonly IFlowFileControllerFactory _flowControllerFactory;
        private readonly IFlowFileLogRepositoryFactory _flowFileLogFactory;
        private readonly IFlowFileControllerServiceFactory _flowControllerServiceFactory;
        private readonly ISpecProviderFactory _specProviderFactory;

        public List<IDisposable> Services { get; }

        public FlowController(
            IFlowFileControllerFactory flowControllerFactory,
            IFlowFileLogRepositoryFactory flowFileLogFactory,
            IFlowFileControllerServiceFactory flowControllerServiceFactory,
            ISpecProviderFactory specProviderFactor) 
        {
            _flowControllerFactory = flowControllerFactory;
            _flowFileLogFactory = flowFileLogFactory;
            _flowControllerServiceFactory = flowControllerServiceFactory;
            _specProviderFactory = specProviderFactor;

            Services = new List<IDisposable>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public void Start(FlowConfiguration config)
        {
            _config = config;

            // initialize directories
            foreach (var definition in _config.Controllers)
            {
                if (!Directory.Exists(definition.Input))
                    Directory.CreateDirectory(definition.Input);
                if (!Directory.Exists(definition.Output))
                    Directory.CreateDirectory(definition.Output);
            }

            foreach (var flowControllerDefinitionse in _config.Controllers.OrderBy(m => m.DateCreated))
                StartController(flowControllerDefinitionse);
        }

        protected void StartController(FlowControllerDefinition definition)
        {
            var batchControllerType = Type.GetType(definition.TypeName);

            // ReSharper disable once UsePatternMatching
            IFlowFileController flowController = _flowControllerFactory.Create(batchControllerType);
            if (flowController == null)
                throw new InvalidOperationException($"Controller type does not implement {typeof(IFlowFileController).Name}");

            // TODO add rules
            //if(definition.FieldValidationRules != null)
            //    foreach (FlowFieldValidation rule in definition.FieldValidationRules)
            //        flowController.AddRule(rule);
            

            // configure
            flowController.InDirectory = definition.Input;
            flowController.OutDirectory = definition.Output;
            flowController.FlowId = _config.FlowId;
            flowController.ControllerName = definition.ControllerName;

            // apply controller configuration details
            if (definition.ConfigurationDetails != null)
            {
                Type type = flowController.GetType();

                foreach (var configDetail in definition.ConfigurationDetails)
                {
                    var property = type.GetProperty(configDetail.Key);
                    if(property != null)
                    {
                        if (property.GetSetMethod() != null)
                        {
                            property.SetValue(flowController, configDetail.Value);
                        }
                    }
                }
            }

            // resolve repository to store flow file process details
            var logRepository = _flowFileLogFactory.Get();
            logRepository.WorkingFolder = _config.BasePath;

            FlowFileControllerService processor = _flowControllerServiceFactory.Get(logRepository, flowController);

            // configure polling internval
            processor.Interval = TimeSpan.FromSeconds(definition.PollInterval);

            // log how many files were processed
            processor.FlowFileProcessed += (o, e) =>
            {
                Processed++;
            };

            Services.Add(processor);

            // start reading from incoming data source
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