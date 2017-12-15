using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;
using System;
using System.Linq;

namespace FutureState.Domain.Providers
{
    public class StakeholderLoginProvider : ProviderLinq<StakeholderLogin, Guid>
    {
        public StakeholderLoginProvider(
            IUnitOfWorkLinq<StakeholderLogin, Guid> db,
            IEntityIdProvider<StakeholderLogin, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<StakeholderLogin> specProvider = null,
            EntityHandler<StakeholderLogin,Guid> entityHandler = null)
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
        }

        /// <summary>
        ///     Gets a stakeholder login entry by external id and scenario.
        /// </summary>
        public StakeholderLogin GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }
    }
}