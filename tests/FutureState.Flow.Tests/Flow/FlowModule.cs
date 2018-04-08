using Autofac;
using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using FutureState.Specifications;
using System;

namespace FutureState.Flow.Tests.Flow
{
    public class FlowModule : Module
    {
        protected override void Load(ContainerBuilder cb)
        {
            base.Load(cb);

            cb.RegisterType<FlowIdRepo>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowService>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileBatchControllerFactory>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileLogRepositoryFactory>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileControllerServiceFactory>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowController>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileLogRepo>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileControllerService>()
                .AsSelf().AsImplementedInterfaces();

            cb.RegisterGeneric(typeof(ProcessorConfiguration<,>))
                .SingleInstance() // make singleton
                .AsSelf();

            cb.RegisterGeneric(typeof(FlowFileController<,>))
                .AsSelf();

            cb.RegisterGeneric(typeof(Processor<,>))
                .AsSelf();

            cb.RegisterGeneric(typeof(ProcessorEngine<>))
                .AsSelf();

            cb.RegisterGeneric(typeof(SpecProvider<>))
                .AsSelf()
                .As(typeof(IProvideSpecifications<>))
                .SingleInstance();

            cb.RegisterType<SpecProviderFactory>()
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

        public class FlowFileBatchControllerFactory : IFlowFileControllerFactory
        {
            private readonly IComponentContext _context;

            public FlowFileBatchControllerFactory(IComponentContext context)
            {
                _context = context;
            }

            public IFlowFileController Create(Type type)
            {
                // ReSharper disable once UsePatternMatching
                var batchProcessor = _context.Resolve(type) as IFlowFileController;
                if (batchProcessor == null)
                    throw new InvalidOperationException(
                        $"Controller type does not implement {typeof(IFlowFileController).Name}");

                return batchProcessor;
            }
        }

        public class FlowFileLogRepositoryFactory : IFlowFileLogRepositoryFactory
        {
            private readonly IComponentContext _context;

            public FlowFileLogRepositoryFactory(IComponentContext context)
            {
                _context = context;
            }

            public FlowFileLogRepo Get()
            {
                return _context.Resolve<FlowFileLogRepo>();
            }
        }

        public class FlowFileControllerServiceFactory : IFlowFileControllerServiceFactory
        {
            private readonly IComponentContext _context;

            public FlowFileControllerServiceFactory(IComponentContext context)
            {
                _context = context;
            }

            public FlowFileControllerService Get(IFlowFileLogRepo repository, IFlowFileController controller)
            {
                return _context.Resolve<FlowFileControllerService>(
                    new TypedParameter(typeof(IFlowFileLogRepo), repository),
                    new TypedParameter(typeof(IFlowFileController), controller));
            }
        }
    }
}