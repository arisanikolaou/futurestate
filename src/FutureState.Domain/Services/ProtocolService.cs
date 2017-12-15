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
    ///     Protocols are the data structures that are exchanged on interfaces.
    /// </remarks>
    public class ProtocolService : FsService<Protocol>
    {
        private readonly ProjectService _scenarioProvider;
        private readonly StakeholderService _stakeholderService;

        public ProtocolService(
            FSSecurityContext securityContext,
            ProjectService projectService,
            StakeholderService stakeholderService,
            ReferenceProvider referenceService,
            UnitOfWorkLinq<Protocol, Guid> db,
            IEntityIdProvider<Protocol, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Protocol> specProvider = null,
            EntityHandler<Protocol, Guid> entityHandler = null) : 
            base(securityContext, referenceService, db, idProvider, messagePipe, specProvider, entityHandler)
        {
            _scenarioProvider = projectService;
            _stakeholderService = stakeholderService;
        }

        /// <summary>
        ///     Gets the protocol that contains the current instance.
        /// </summary>
        public Protocol GetContainer(Protocol protocol)
        {
            Guard.ArgumentNotNull(protocol, nameof(protocol));

            return protocol.ParentId.HasValue ? GetById(protocol.ParentId.Value) : null;
        }

        public override Protocol Initialize(Protocol entity)
        {
            // load container from database
            entity.SetServices(new Protocol.DomainServices(() => GetContainer(entity)));

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets a protocol belonging to the scenario. 
        /// </summary>
        public Protocol GetByExternalId(string externalId)
        {
            return GetByExternalId(externalId, null);
        }

        /// <summary>
        ///     Gets a protocol by its external key belonging to a scenario.
        /// </summary>
        public Protocol GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a scenario by its external key belonging to the default scenario.
        /// </summary>
        public Scenario GetScenario(string scenarioExternalId)
        {
            return _scenarioProvider.GetScenario(scenarioExternalId);
        }

        /// <summary>
        ///     Gets a stakeholder by its external id and scenario id.
        /// </summary>
        public Stakeholder GetStakeholder(string stakeholderExternalId, Guid? scenarioId)
        {
            return this._stakeholderService.GetByExternalId(stakeholderExternalId, scenarioId);
        }

        protected override void OnBeforeAdd(Protocol entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }
    }
}