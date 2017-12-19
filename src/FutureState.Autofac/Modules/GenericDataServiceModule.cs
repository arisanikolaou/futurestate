using Autofac;
using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Specifications;

namespace FutureState.Autofac.Modules
{
    /// <summary>
    ///     Regigsters the basic modules required to
    ///  support the data access architecture.
    /// </summary>
    public class GenericDataServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //generic service
            builder.RegisterGeneric(typeof(ProviderLinq<,>))
                .AsSelf();

            //  spec provider
            builder.RegisterGeneric(typeof(SpecProvider<>))
                .As(typeof(IProvideSpecifications<>));

            //  register message pipe
            builder.Register(m => new MessagePipe())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance()
                .PreserveExistingDefaults();

            builder.RegisterGeneric(typeof(EntityIdProvider<,>))
                .As(typeof(IEntityIdProvider<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(DefaultKeyGenerator<,>))
                .As(typeof(IKeyGenerator<,>))
                .SingleInstance();
        }
    }
}