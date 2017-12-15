using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Providers
{
    public class PolicyProvider : ProviderLinq<Policy, Guid>
    {
        private readonly ReferenceProvider _referencesProvider;
        private readonly IUnitOfWorkLinq<Policy, Guid> _db;

        public PolicyProvider(
            IUnitOfWorkLinq<Policy, Guid> db,
            ReferenceProvider referencesProvider,
            IEntityIdProvider<Policy, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Policy> specProvider = null,
            EntityHandler<Policy, Guid> entityHandler = null) : 
            base(db, idProvider, messagePipe, specProvider, entityHandler)
        {
            _referencesProvider = referencesProvider;
            _db = db;
        }

        /// <summary>
        ///     Getsa policy by its external id.
        /// </summary>
        public Policy GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Flush fills references.
        /// </summary>
        public void UpdateReferences(IEnumerable<Reference> references)
        {
            _referencesProvider.FlushFill(references);
        }

        public Policy GetContainingPolicy(Policy policy)
        {
            if (!policy.ContainerId.HasValue)
                return null;

            return GetById(policy.ContainerId.Value);
        }


        public Policy GetContainingPolicy(Policy policy, IUnitOfWorkLinq<Policy, Guid> db)
        {
            if (!policy.ContainerId.HasValue)
                return null;

            return GetById(policy.ContainerId.Value, db);
        }

        protected override void OnBeforeAdd(Policy entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);

            DemandLineageIntegrity(entity);
        }

        protected override void OnBeforeUpdate(Policy entity)
        {
            base.OnBeforeUpdate(entity);

            DemandLineageIntegrity(entity);
        }

        protected void DemandLineageIntegrity(Policy entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            if (entity.ContainerId == null)
                return;

            // should never happend as it will be caught by a simple validation rule
            if (entity.ContainerId == entity.Id)
            {
                string msg = $"The policy {entity.DisplayName} already exists in the instance's own lineage.";
                throw new RuleException(msg);
            }

            // assume that session is open and validate its lineage
            // already checked parent, start with parent and iterate recursively
            var container = GetById(entity.ContainerId.Value, _db);
            while (container != null)
            {
                if (container.Id == entity.Id)
                {
                    string msg = $"The policy {entity.DisplayName} already exists in the instance's own lineage.";
                    throw new InvalidOperationException(msg);
                }

                container = GetContainingPolicy(entity, _db);
            }
        }
    }
}