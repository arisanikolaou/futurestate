using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Services;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Providers
{
    // supports table per hierarchy

    /// <summary>
    ///     Service to add/remove and view references to a given entity.
    /// </summary>
    public class ReferenceProvider : ProviderLinq<Reference, Guid>, IService
    {
        private readonly IUnitOfWorkLinq<Reference, Guid> _db;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="db">The references database.</param>
        /// <param name="keyBinder"></param>
        /// <param name="messagePipe"></param>
        /// <param name="referencesSpec">Rules to validate a given reference entity.</param>
        public ReferenceProvider(
            IUnitOfWorkLinq<Reference, Guid> db,
            IEntityIdProvider<Reference, Guid> keyBinder,
            IMessagePipe messagePipe,
            IProvideSpecifications<Reference> referencesSpec = null,
            EntityHandler<Reference, Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, referencesSpec, entityHandler)
        {
            Guard.ArgumentNotNull(db, nameof(db));

            _db = db;
        }

        /// <summary>
        ///     Gets all reference identified by a given container/reference id.
        /// </summary>
        /// <param name="referenceId">The entity id that owns the reference.</param>
        public IList<Reference> GetReferences(Guid referenceId) => base.Where(m => m.ReferenceId == referenceId).ToList();

        /// <summary>
        ///     Flush fills references.
        /// </summary>
        /// <param name="references"></param>
        public void FlushFill(IEnumerable<Reference> references)
        {
            Guard.ArgumentNotNull(references, nameof(references));

            var referencesToRemove = references
                .Select(m => m.ReferenceId)
                .Distinct();

            foreach (Guid reference in referencesToRemove)
                RemoveReferences(reference);

            Add(references);
        }

        /// <summary>
        ///     Removes all references associated by a given reference id.
        /// </summary>
        public void RemoveReferences(Guid referenceId)
        {
            using (_db.Open())
            {
                RemoveReferences(referenceId, _db);

                _db.Commit();
            }
        }

        /// <summary>
        ///     Removes all references associated to a given entity.
        /// </summary>
        public void RemoveReferences(Guid referenceId, IUnitOfWorkLinq<Reference, Guid> db)
        {
            db.EntitySet.BulkDeleter.Delete(m => m.ReferenceId == referenceId);
        }

        protected override void OnBeforeAdd(Reference entity)
        {
            // fixup description
            entity.Description = entity.Description ?? "";

            base.OnBeforeAdd(entity);
        }

        public void RemoveByScenario(Scenario source)
        {
            this.Remove(m => m.ScenarioId == source.Id);
        }

        public IEnumerable<Reference> GetByScenario(Scenario source)
        {
            return this.Where(m => m.ScenarioId == source.Id);
        }
    }
}