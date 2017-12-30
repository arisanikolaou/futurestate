using Autofac;
using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Providers;
using FutureState.Specifications;

namespace FutureState.Autofac.Modules
{
    /// <summary>
    ///     Regigsters the basic modules required to
    ///     support the data access architecture.
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
                .AsSelf()
                .As(typeof(IProvideSpecifications<>))
                .SingleInstance();

            //  register message pipe
            builder.Register(m => new MessagePipe())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance()
                .PreserveExistingDefaults();

            builder.RegisterGeneric(typeof(KeyProvider<,>))
                .As(typeof(KeyProvider<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(KeyGenerator<,>))
                .As(typeof(IKeyGenerator<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(KeyBinderFromAttributes<,>))
                .As(typeof(IKeyBinder<,>))
                .SingleInstance();
        }
    }
}