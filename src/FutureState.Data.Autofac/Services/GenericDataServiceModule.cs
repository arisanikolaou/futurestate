using System;
using Autofac;
using FutureState.ComponentModel;
using FutureState.Data.Providers;
using FutureState.Specifications;
using FutureState.Data.Keys;

namespace FutureState.Data.Autofac.Services
{
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

            builder.Register(m => new KeyGetter<Guid>(SeqGuid.Create))
                .As(typeof(IKeyGetter<Guid>))
                .SingleInstance()
                .PreserveExistingDefaults();

            long current = 0;//TODO: optimize
            builder.Register(m => new KeyGetter<string>((current ++).ToString))
                .As(typeof(IKeyGetter<string>))
                .SingleInstance()
                .PreserveExistingDefaults();
        }
    }
}
