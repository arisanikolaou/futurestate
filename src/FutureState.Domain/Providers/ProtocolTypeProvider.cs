using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;

namespace FutureState.Domain.Providers
{
    public class ProtocolTypeProvider : ProviderLinq<ProtocolType, string>
    {
        public ProtocolTypeProvider(
            IUnitOfWorkLinq<ProtocolType, string> db,
            IEntityIdProvider<ProtocolType, string> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<ProtocolType> specProvider = null,
            EntityHandler<ProtocolType, string> entityHandler = null) : 
            base(db, idProvider, messagePipe, specProvider, entityHandler)
        {
        }
    }
}