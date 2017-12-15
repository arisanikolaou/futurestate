using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;

namespace FutureState.Domain.Providers
{
    public class LifeCycleProvider : ProviderLinq<LifeCycle, string>
    {
        public LifeCycleProvider(
            IUnitOfWorkLinq<LifeCycle, string> db,
            IEntityIdProvider<LifeCycle, string> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<LifeCycle> specProvider = null,
            EntityHandler<LifeCycle, string> onBeforeInsert = null)
            : base(db, keyBinder, messagePipe, specProvider, onBeforeInsert)
        {
        }
    }
}
