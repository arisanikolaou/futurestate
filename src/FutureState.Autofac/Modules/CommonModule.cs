using Autofac;
using FutureState.Specifications;

namespace FutureState.Autofac.Modules
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(SpecProvider<>))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SpecProviderFactory>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        public class SpecProviderFactory : ISpecProviderFactory
        {
            private readonly IComponentContext _context;

            public SpecProviderFactory(IComponentContext context)
            {
                _context = context;
            }

            public SpecProvider<TEntityOrService> Get<TEntityOrService>()
            {
                return _context.Resolve<SpecProvider<TEntityOrService>>();
            }
        }
    }
}
