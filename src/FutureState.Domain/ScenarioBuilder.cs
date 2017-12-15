using System.Collections.Generic;
using FutureState.Domain.Providers;
using NLog;

namespace FutureState.Domain
{
    /// <summary>
    ///     Creates a scenario container for all architectural artifacts
    /// </summary>
    public class ScenarioBuilder
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly List<IScenarioBuilder> _services;
        private readonly ReferenceProvider _referenceService;
        private readonly CapabilityProvider _capabilityService;
        private readonly ScenarioProvider _scenarioProvider;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ScenarioBuilder(
            ScenarioProvider scenarioProvider,
            ReferenceProvider referenceService,
            CapabilityProvider capabilityService,
            ScenarioBuilderServiceList serviceList)
        {
            _services = new List<IScenarioBuilder>();

            _scenarioProvider = scenarioProvider;
            _referenceService = referenceService;
            _capabilityService = capabilityService;

            _services.AddRange(serviceList.List);
        }

        /// <summary>
        ///     Copies architectural artefacts from one scenario to another.
        /// </summary>
        public void Initialize(Scenario referenceScenario, Scenario targetScenario)
        {
            // do outside of a transaction
            foreach (var service in _services)
                service.CopyToScenario(referenceScenario, targetScenario);

            // mark the scenario initialized
            targetScenario.SetInitialized();

            // update state
            _scenarioProvider.Update(targetScenario);
        }

        /// <summary>
        ///     Removes the scenario and all related entities.
        /// </summary>
        public void Remove(Scenario referenceScenario)
        {
            // todo: wrap in a transaction
            foreach (var service in _services)
                service.Remove(referenceScenario);

            _referenceService.RemoveByScenario(referenceScenario);
            _capabilityService.RemoveByScenario(referenceScenario);
        }
    }
}