using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Specifications;
using System;
using System.Linq;

namespace FutureState.Domain.Services
{
    public class PolicyService : ProviderLinq<Policy, Guid>
    {
        private ReferenceProvider _referenceService;
        private FsUnitOfWork _fsDb;
        private readonly DesignDomainService _designDomainService;

        public PolicyService(
            FsUnitOfWork db,
            DesignDomainService designDomainService,
            ReferenceProvider referencesProvider,
            IEntityIdProvider<Policy, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Policy> specProvider = null,
            EntityHandler<Policy, Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(referencesProvider, nameof(referencesProvider));

            _referenceService = referencesProvider;
            _fsDb = db;
            _designDomainService = designDomainService;
        }

        public override Policy Initialize(Policy entity)
        {
            var services = new Policy.DomainServices(
                () => this._referenceService.GetReferences(entity.Id),
                () => this.Where(m => m.ContainerId == entity.Id),
                () => entity.DesignDomainId.HasValue ? this._designDomainService.GetById(entity.DesignDomainId.Value) : null);

            entity.SetServices(services);

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets a policy by external id or null if it doesn't exist.
        /// </summary>
        public Policy GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId)
                .FirstOrDefault() ;
        }
    }
}
