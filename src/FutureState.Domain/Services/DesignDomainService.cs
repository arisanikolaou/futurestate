using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;
using System;
using System.Linq;
using System.Collections.Generic;
using FutureState.Domain.Data;

namespace FutureState.Domain.Providers
{
    public class DesignDomainService : ProviderLinq<DesignDomain, Guid>
    {
        private readonly ReferenceProvider _referenceService;
        readonly FsUnitOfWork _fsDb;
        private readonly PolicyProvider _policyProvider;

        public DesignDomainService(
            FsUnitOfWork db,
            PolicyProvider policyProvider,
            ReferenceProvider referencesProvider,
            IEntityIdProvider<DesignDomain, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<DesignDomain> specProvider = null,
            EntityHandler<DesignDomain, Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
            _referenceService = referencesProvider;
            _fsDb = db;
            _policyProvider = policyProvider;
        }

        protected override void OnBeforeAdd(DesignDomain entity)
        {
            base.OnBeforeAdd(entity);

            // fixup date created
            if(!entity.DateCreated.HasValue)
                entity.DateCreated = DateTime.UtcNow;
        }

        public Policy GetPolicyByCode(string policyCode)
        {
            return _policyProvider.GetByExternalId(policyCode);
        }


        protected override void OnBeforeDelete(Guid key)
        {
            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Gets all policies that are associated with a given domain.
        /// </summary>
        public IEnumerable<Policy> GetPolicies(DesignDomain domain)
        {
            Guard.ArgumentNotNull(domain, nameof(domain));

            IList<Guid> policyIds = null;
            using (_fsDb.Open())
            {
                policyIds = _fsDb.Policies.LinqReader.Where(m => m.DesignDomainId == domain.Id)
                    .Select(m => m.Id).ToList();
            }

            return _policyProvider.GetByIds(policyIds);
        }

        public override DesignDomain Initialize(DesignDomain entity)
        {
            // inject domain context
            var domainService = new DesignDomain.DomainService(
                () => GetContainingDomain(entity),
                () => GetPolicies(entity));

            entity.SetServices(domainService);

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets a domain by its external id.
        /// </summary>
        public DesignDomain GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Add references to a given domain.
        /// </summary>
        public void AddReferences(DesignDomain domain, IEnumerable<Reference> references)
        {
            Guard.ArgumentNotNull(domain, nameof(domain));
            Guard.ArgumentNotNull(references, nameof(references));

            // assign references
            references.Each(m => m.ReferenceId = domain.Id);

            this._referenceService.Add(references);
        }

        /// <summary>
        ///     Flush fill references.
        /// </summary>
        public void UpdateReferences(DesignDomain domain, IEnumerable<Reference> references)
        {
            Guard.ArgumentNotNull(domain, nameof(domain));
            Guard.ArgumentNotNull(references, nameof(references));

            // assign references
            references.Each(m => m.ReferenceId = domain.Id);

            this._referenceService.RemoveReferences(domain.Id);
            this._referenceService.Update(references);
        }

        protected void DemandLineageIntegrity(DesignDomain entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            if (entity.ParentId == null)
                return;

            // should never happend as it will be caught by a simple validation rule
            if (entity.ParentId == entity.Id)
            {
                string msg = $"The domain {entity.DisplayName} already exists in the instance's own lineage.";
                throw new RuleException(msg);
            }

            // assume that session is open and validate its lineage
            // already checked parent, start with parent and iterate recursively
            var container = GetById(entity.ParentId.Value, _fsDb);
            while (container != null)
            {
                if (container.Id == entity.Id)
                {
                    string msg = $"The domain {entity.DisplayName} already exists in the instance's own lineage.";
                    throw new InvalidOperationException(msg);
                }

                container = GetContainingDomain(entity, _fsDb);
            }
        }

        /// <summary>
        ///     Gets the containing design domain e.g. software -> linux.
        /// </summary>
        public DesignDomain GetContainingDomain(DesignDomain entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            if (entity.ParentId == null)
                return null;

            return Where(m => m.Id == entity.ParentId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the containing design domain e.g. software -> linux.
        /// </summary>
        public DesignDomain GetContainingDomain(DesignDomain entity, FsUnitOfWork fsDb)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            if (entity.ParentId == null)
                return null;

            return Where(m => m.Id == entity.ParentId, fsDb).FirstOrDefault();
        }
    }
}
