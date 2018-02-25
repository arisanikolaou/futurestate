using FutureState.Flow.Controllers;
using FutureState.Reflection;
using FutureState.Specifications;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FutureState.Flow
{
    /// <summary>
    ///     Controls how flow files are processed.
    /// </summary>
    public class FlowController : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, Type> _dictOfControllerTypes;
        private readonly IFlowFileControllerFactory _flowControllerFactory;
        private readonly IList<IFlowFileController> _flowControllers;
        private readonly IFlowFileControllerServiceFactory _flowControllerServiceFactory;
        private readonly IFlowFileLogRepositoryFactory _flowFileLogFactory;
        private readonly ISpecProviderFactory _specProviderFactory;

        private FlowConfiguration _config;
        private FlowFileControllerService _processor;
        private bool _started;

        /// <summary>
        ///     Setup static dictionary of types that implement IFlowFileController
        /// </summary>
        static FlowController()
        {
            // scan assemblies
            var appTypeScanner = AppTypeScanner.Default;

            var controllerTypes = appTypeScanner
                .GetTypes<IFlowFileController>()
                .ToList();

            _dictOfControllerTypes = new Dictionary<string, Type>();

            // controller types
            foreach (var controllerType in controllerTypes)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _dictOfControllerTypes.Add(controllerType.Value.AssemblyQualifiedName, controllerType.Value);

                var attribute = controllerType.Value.GetCustomAttribute<DisplayNameAttribute>();
                if (attribute != null)
                    _dictOfControllerTypes[attribute.DisplayName] = controllerType.Value;
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
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
        ///     Gets the number of flow file processed.
        /// </summary>
        public long Processed { get; private set; }

        /// <summary>
        ///     Disposes.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        ///     Starts the flow.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public void Start(FlowConfiguration config)
        {
            Guard.ArgumentNotNull(config, nameof(config));

            _config = config;

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Starting all controllers.");

            // start controllers in sequential order
            foreach (var flowControllerDefinitionse in _config.Controllers
                .OrderBy(m => m.ExecutionOrder))
            {
                StartController(flowControllerDefinitionse);
            }

            _started = true;

            if (_logger.IsDebugEnabled)
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

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Starting controller {definition.ControllerName}");

            //flow controller type
            Type flowControllerType;

            // resolve from precompiled list of well known processors
            // or assembly resolve
            if (_dictOfControllerTypes.ContainsKey(definition.ControllerName))
                flowControllerType = _dictOfControllerTypes[definition.ControllerName];
            else
                flowControllerType = Type.GetType(definition.TypeName);

            // ReSharper disable once UsePatternMatching
            IFlowFileController flowController = _flowControllerFactory
                .Create(flowControllerType);

            // flow controller
            if (flowController == null)
                throw new InvalidOperationException(
                    $"Controller type does not implement {typeof(IFlowFileController).Name}");

            // build rules that will be used to validate outgoing entitities
            var specProviderBuilder = new SpecProviderBuilder(_specProviderFactory);

            // spec providers expected to be single instance in
            // the application's scope
            if (definition.FieldValidationRules != null)
            {
                Type type = null;

                if (_logger.IsTraceEnabled)
                    _logger.Trace($"Activating controller type {flowController?.TargetEntityType?.AssemblyQualifiedTypeName}.");

                try
                {
                    type = Type.GetType(flowController.TargetEntityType.AssemblyQualifiedTypeName);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't load type {flowController.TargetEntityType.AssemblyQualifiedTypeName}.");
                }

                specProviderBuilder.Build(
                    type,
                    definition.FieldValidationRules.ToList());
            }

            // configure
            flowController.InDirectory = definition.Input;
            flowController.OutDirectory = definition.Output;

            flowController.Flow = _config.Flow;
            flowController.ControllerName = definition.ControllerName;

            // apply controller configuration details
            if (definition.ConfigurationDetails != null)
            {
                var type = flowController.GetType();

                if (_logger.IsDebugEnabled)
                    _logger.Debug($"Configuring properties of controller type {type.Name} has been initialized.");

                foreach (var configDetail in definition.ConfigurationDetails)
                {
                    var property = type.GetProperty(configDetail.Key);
                    if (property != null)
                        if (property.GetSetMethod() != null)
                            property.SetValue(flowController, configDetail.Value);
                }
            }

            // complete initialization
            flowController.Initialize(); 

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Flow controller {definition.ControllerName} has been initialized.");


            FlowFileControllerService processor = GetControllerService(flowController);

            // configure polling internval
            processor.Interval = TimeSpan.FromSeconds(definition.PollInterval);

            // log how many files were processed
            processor.FlowFileProcessed += (o, e) => { Processed++; };

            // start reading from incoming data source
            processor.Start();

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Flow controller {definition.ControllerName} has started.");

            // active processor
            _processor = processor;

            // add to collection of initialized controllers
            _flowControllers.Add(flowController);
        }

        FlowFileControllerService GetControllerService(IFlowFileController flowController)
        {
            // resolve repository to store flow file process details
            var logRepository = _flowFileLogFactory.Get();

            // set base path for the flow
            logRepository.DataDir = _config.BasePath;

            FlowFileControllerService processor = _flowControllerServiceFactory.Get(logRepository, flowController);

            return processor;
        }

        /// <summary>
        ///     Stop processing data from the incoming data store.
        /// </summary>
        public void Stop()
        {
            if (!_started)
                return; // already stopped

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Stopping flow {_config.Flow}.");

            _processor?.Dispose();

            _flowControllers.Each(m => m.Dispose());

            if (_logger.IsDebugEnabled)
                _logger.Debug($"Stopped flow {_config.Flow}.");

            _started = false;
        }
    }
}