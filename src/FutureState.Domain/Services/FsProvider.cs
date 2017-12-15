using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Services to add/remove and query a given entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class FsService<TEntity> : ProviderLinq<TEntity, Guid>
        where TEntity : class, IEntityMutableKey<Guid>, IDesignArtefact
    {
        private readonly FSSecurityContext _securityContext;
        private readonly ReferenceProvider referenceProvider;

        public FsService(
            FSSecurityContext securityContext,
            ReferenceProvider referenceProvider,
            IUnitOfWorkLinq<TEntity, Guid> db,
            IEntityIdProvider<TEntity, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<TEntity> specProvider = null,
            EntityHandler<TEntity,Guid> entityHandler = null) :
            base(db, idProvider, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(db, nameof(db));
            Guard.ArgumentNotNull(referenceProvider, nameof(referenceProvider));
            Guard.ArgumentNotNull(securityContext, nameof(securityContext));

            _securityContext = securityContext;
            this.referenceProvider = referenceProvider;
        }

        /// <summary>
        ///     Gets the active user ad.
        /// </summary>
        public Guid UserId => _securityContext.GetCurrentAppPrinciple().FSIdentity.UserId;

        /// <summary>
        ///     Gets the user in the context of the given provider.
        /// </summary>
        /// <returns></returns>
        public override string GetCurrentUser()
        {
            return _securityContext.GetCurrentAppPrinciple()?.Identity?.Name ?? base.GetCurrentUser();
        }
        
        /// <summary>
        ///     Gets the service to query, add and remove documentation references.
        /// </summary>
        /// <returns></returns>
        public ReferenceProvider GetReferencesProvider() => referenceProvider;

        /// <summary>
        ///     Gets the security context active in the service.
        /// </summary>
        /// <returns></returns>
        public FSSecurityContext GetSecurityContext() => _securityContext;

        /// <summary>
        ///     Adds a set of references.
        /// </summary>
        public void FlushFill(IEnumerable<Reference> references)
        {
            Guard.ArgumentNotNull(references, nameof(references));

            GetReferencesProvider().FlushFill(references);
        }

        /// <summary>
        ///     Adds a set of references to a given entity.
        /// </summary>
        public void FlushFill(TEntity entity, IEnumerable<Reference> references)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            Guard.ArgumentNotNull(references, nameof(references));

            references.Each(m => m.ReferenceId = entity.Id);

            GetReferencesProvider().FlushFill(references);
        }

        /// <summary>
        ///     Gets all entities that belong to a given scenario.
        /// </summary>
        /// <param name="scenario">If null indicates the default scenario.</param>
        public virtual IList<TEntity> GetByScenario(Scenario scenario)
        {
            if (scenario != null)
                return Where(m => m.ScenarioId == scenario.Id).ToList();
            else
                return Where(m => m.ScenarioId == null).ToList();
        }
    }
}