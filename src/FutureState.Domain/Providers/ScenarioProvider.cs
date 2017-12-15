using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Security;
using FutureState.Specifications;
using System;
using System.Linq;

namespace FutureState.Domain.Providers
{
    /// <summary>
    ///     Service to add/remove and update scenarios.
    /// </summary>
    public class ScenarioProvider : ProviderLinq<Scenario, Guid>
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ScenarioProvider(
            IUnitOfWorkLinq<Scenario,Guid> db,
            IEntityIdProvider<Scenario, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Scenario> specProvider = null,
            EntityHandler<Scenario,Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
            
        }

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Deletes all scenarios associated with a given project by its project key.
        /// </summary>
        public void DeleteByProject(Guid projectId)
        {
            using (Db.Open())
            {
                this.Db.EntitySet.BulkDeleter.Delete(m => m.ProjectId == projectId);

                Db.Commit();
            }
        }


        /// <summary>
        ///     Deletes all scenarios associated with a given project by its key and defers commiting changes to the caller.
        /// </summary>
        public void DeleteByProject(Guid projectId, IUnitOfWorkLinq<Scenario, Guid> db)
        {
            db.EntitySet.BulkDeleter.Delete(m => m.ProjectId == projectId);
        }
    }
}
