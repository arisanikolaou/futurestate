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
    /// <summary>
    ///     Service to add/remove and query cabilities.
    /// </summary>
    public class CapabilityProvider : ProviderLinq<Capability, Guid>
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CapabilityProvider(
            IUnitOfWorkLinq<Capability,Guid> db,
            IEntityIdProvider<Capability, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Capability> specProvider = null,
            EntityHandler<Capability,Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
        }

        /// <summary>
        ///     Remove all capabilities associated with a given reference id.
        /// </summary>
        public void RemoveAll(Guid referenceId)
        {
            using (this.Db.Open())
            {
                this.Db.EntitySet.BulkDeleter.Delete(m => m.SoftwareModelId == referenceId);

                this.Db.Commit();
            }
        }

        /// <summary>
        ///     Gets all capabilities associated to a given entity by its entity id.
        /// </summary>
        public IList<Capability> GetCapabilities(Guid referenceId)
        {
            using (Db.Open())
            {
                return Db.EntitySet.LinqReader.Where(m => m.SoftwareModelId == referenceId)
                    .Select(Initialize) // intiialize
                    .ToList();
            }
        }

        /// <summary>
        ///     Removes all capabilities associated with a given scenario.
        /// </summary>
        public void RemoveByScenario(Scenario source) => this.Remove(m => m.ScenarioId == source.Id);

        /// <summary>
        ///     Gets all capabilities associated with a given scenario.
        /// </summary>
        public IEnumerable<Capability> GetByScenario(Scenario source)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            return this.Where(m => m.ScenarioId == source.Id);
        }

        /// <summary>
        ///     Gets a capability by external key and scenario.
        /// </summary>
        public Capability GetByExternalId(string externalId, Guid? scenarioId) => this.Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();

        /// <summary>
        ///     Removes all capabilities associated with a given architectural model.
        /// </summary>
        public void FlushFill(IEnumerable<Capability> capabilities)
        {
            using (Db.Open())
            {
                foreach (var capabilityId in capabilities.Select(m => m.SoftwareModelId).Distinct())
                    Db.EntitySet.BulkDeleter.Delete(m => m.SoftwareModelId == capabilityId);

                Add(capabilities, Db);
            }
        }
    }
}