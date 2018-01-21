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
    public class GenericDataAccessModule : Module
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

            builder.RegisterGeneric(typeof(UnitOfWork<,>))
                .Named("Default", typeof(UnitOfWork<,>))
                .As(typeof(IUnitOfWork<,>))
                .AsSelf();

            builder.RegisterGeneric(typeof(UnitOfWorkLinq<,>))
                .Named("Default", typeof(UnitOfWorkLinq<,>))
                .As(typeof(IUnitOfWorkLinq<,>))
                .AsSelf();

            builder.RegisterGeneric(typeof(KeyProvider<,>))
                .AsSelf()
                .As(typeof(IKeyProvider<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(KeyGenerator<,>))
                .AsSelf()
                .As(typeof(IKeyGenerator<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(KeyBinderFromAttributes<,>))
                .As(typeof(IKeyBinder<,>))
                .SingleInstance();
        }
    }
}