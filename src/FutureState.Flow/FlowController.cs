using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using FutureState.Reflection;
using NLog;

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

    /// <summary>
    ///     Controls how flow files are processed.
    /// </summary>
    public class FlowController : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the number of flow file processed.
        /// </summary>
        public long Processed { get; private set; }

        private FlowConfiguration _config;
        private FlowFileControllerService _processor;
        private readonly List<IFlowFileController> _flowControllers;
        private readonly IFlowFileControllerFactory _flowControllerFactory;
        private readonly IFlowFileLogRepositoryFactory _flowFileLogFactory;
        private readonly IFlowFileControllerServiceFactory _flowControllerServiceFactory;
        private readonly ISpecProviderFactory _specProviderFactory;
        private bool _started;
        private static readonly Dictionary<string, Type> _dictTypes;

        static FlowController()
        {
            // scan assemblies
            AppTypeScanner appTypeScanner = AppTypeScanner.Default;

            List<Lazy<Type>> controllerTypes = appTypeScanner
                .GetTypes<IFlowFileController>()
                .ToList();

            _dictTypes = new Dictionary<string, Type>();
            foreach (var controllerType in controllerTypes)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dictTypes.Add(controllerType.Value.AssemblyQualifiedName, controllerType.Value);

                var attribute = controllerType.Value.GetCustomAttribute<DisplayNameAttribute>();
                if (attribute != null)
                    _dictTypes[attribute.DisplayName] = controllerType.Value;
            }

        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="flowControllerFactory"></param>
        /// <param name="flowFileLogFactory"></param>
        /// <param name="flowControllerServiceFactory"></param>
        /// <param name="specProviderFactor"></param>
        public FlowController(
            IFlowFileControllerFactory flowControllerFactory,
            IFlowFileLogRepositoryFactory flowFileLogFactory,
            IFlowFileControllerServiceFactory flowControllerServiceFactory,
            ISpecProviderFactory specProviderFactor)
        {
            Guard.ArgumentNotNull(flowControllerFactory, nameof(flowControllerFactory));
            Guard.ArgumentNotNull(flowFileLogFactory, nameof(flowFileLogFactory));
            Guard.ArgumentNotNull(flowControllerServiceFactory, nameof(flowControllerServiceFactory));
            Guard.ArgumentNotNull(specProviderFactor, nameof(specProviderFactor));

            _flowControllerFactory = flowControllerFactory;
            _flowFileLogFactory = flowFileLogFactory;
            _flowControllerServiceFactory = flowControllerServiceFactory;
            _specProviderFactory = specProviderFactor;

            _flowControllers = new List<IFlowFileController>();
        }

        /// <summary>
        ///     Starts the flow.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public void Start(FlowConfiguration config)
        {
            Guard.ArgumentNotNull(config, nameof(config));

            _config = config;

            // start controllers in sequential order
            foreach (var flowControllerDefinitionse in _config.Controllers.OrderBy(m => m.DateCreated))
                StartController(flowControllerDefinitionse);

            _started = true;

            if(_logger.IsDebugEnabled)
                _logger.Debug($"Started all controllers.");
        }

        /// <summary>
        ///     Gets all flow controllers that have been started and configured.
        /// </summary>
        public IFlowFileController[] GetControllers()
        {
            return _flowControllers.ToArray();
        }

        /// <summary>
        ///     Starts a controller.
        /// </summary>
        /// <param name="definition">Definition to configure the controller.</param>
        protected void StartController(FlowControllerDefinition definition)
        {
            Guard.ArgumentNotNull(definition, nameof(definition));

            if(_logger.IsDebugEnabled)
                _logger.Debug($"Starting controller {definition.ControllerName}");

            //flow controller type
            Type flowControllerType;

            // resolve from precompiled list of well known processors
            // or assembly resolve
            if (_dictTypes.ContainsKey(definition.ControllerName))
                flowControllerType = _dictTypes[definition.ControllerName];
            else
                flowControllerType = Type.GetType(definition.TypeName);

            // ReSharper disable once UsePatternMatching
            IFlowFileController flowController = _flowControllerFactory
                .Create(flowControllerType);
            if (flowController == null)
                throw new InvalidOperationException(
                    $"Controller type does not implement {typeof(IFlowFileController).Name}");

            // build rules that will be used to validate outgoing entitities
            var specProviderBuilder = new SpecProviderBuilder(_specProviderFactory);

            // spec providers expected to be single instance in
            // the application's scope
            if (definition.FieldValidationRules != null)
                specProviderBuilder.Build(
                    flowController.OutputType,
                    definition.FieldValidationRules.ToList());

            // configure
            flowController.InDirectory = definition.Input;
            flowController.OutDirectory = definition.Output;
            flowController.FlowId = _config.FlowId;
            flowController.ControllerName = definition.ControllerName;

            // apply controller configuration details
            if (definition.ConfigurationDetails != null)
            {
                var type = flowController.GetType();

                foreach (var configDetail in definition.ConfigurationDetails)
                {
                    var property = type.GetProperty(configDetail.Key);
                    if (property != null)
                    {
                        if (property.GetSetMethod() != null)
                        {
                            property.SetValue(flowController, configDetail.Value);
                        }
                    }
                }
            }

            flowController.Initialize(); // complete initialization

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Flow controller {definition.ControllerName} has been initialized.");

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

            // start reading from incoming data source
            processor.Start();

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Flow controller {definition.ControllerName} has started.");

            // active processor
            _processor = processor;

            // add to collection of initialized controllers
            _flowControllers.Add(flowController);
        }

        public void Stop()
        {
            if (!_started)
                return; // already stopped

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Stopping flow {this._config.FlowId}.");

            _processor?.Dispose();

            _flowControllers.Each(m => m.Dispose());

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Stopped flow {this._config.FlowId}.");

            _started = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}