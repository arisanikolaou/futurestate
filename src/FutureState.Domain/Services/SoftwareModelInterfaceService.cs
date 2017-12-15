using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Specifications;
using System;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Service to add/remove and manage protocols.
    /// </summary>
    /// <remarks>
    ///     SoftwareModelInterfaces are the data structures that are exchanged on interfaces.
    /// </remarks>
    public class SoftwareModelInterfaceService : FsService<SoftwareModelInterface>
    {
        private readonly ProtocolService _protocolService;
        private readonly ProjectService _projectService;

        public SoftwareModelInterfaceService(
            FSSecurityContext securityContext,
            ReferenceProvider referencesProvider,
            ProtocolService protocolService,
            ProjectService projectService,
            UnitOfWorkLinq<SoftwareModelInterface, Guid> db,
            IEntityIdProvider<SoftwareModelInterface, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<SoftwareModelInterface> specProvider = null,
            EntityHandler<SoftwareModelInterface, Guid> entityHandler = null) : base(securityContext, referencesProvider, db, idProvider, messagePipe, specProvider,  entityHandler)
        {
            Guard.ArgumentNotNull(protocolService, nameof(protocolService));

            _protocolService = protocolService;
            _projectService = projectService;
        }

        protected override void OnBeforeAdd(SoftwareModelInterface entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }

        /// <summary>
        ///     Gets a software model interface by its an external key.
        /// </summary>
        public SoftwareModelInterface GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a software model interface by its an external key and scenario.
        /// </summary>
        public SoftwareModelInterface GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetScenario(string scenarioExternalId)
        {
            return _projectService.GetScenario(scenarioExternalId);
        }

        /// <summary>
        ///     Gets a protocol by its external key and a scenario id.
        /// </summary>
        public Protocol GetProtocol(string protocolExternalId, Guid? scenarioId)
        {
            return _protocolService.GetByExternalId(protocolExternalId, scenarioId);
        }
    }
}
