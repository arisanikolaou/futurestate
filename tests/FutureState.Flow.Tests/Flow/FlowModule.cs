using System;
using Autofac;
using FutureState.Flow.BatchControllers;
using FutureState.Flow.Data;

namespace FutureState.Flow.Tests.Flow
{
    public class FlowModule : Module
    {
        protected override void Load(ContainerBuilder cb)
        {
            base.Load(cb);

            cb.RegisterType<FlowFileBatchControllerFactory>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileLogRepositoryFactory>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileControllerServiceFactory>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowController>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileLogRepository>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileControllerService>().AsSelf().AsImplementedInterfaces();

            cb.RegisterGeneric(typeof(ProcessorConfiguration<,>))
                .AsSelf()
                .AsImplementedInterfaces();
            
            cb.RegisterGeneric(typeof(FlowFileFlowFileBatchController<,>))
                .AsSelf();

            cb.RegisterGeneric(typeof(Processor<,>))
                .AsSelf();

            cb.RegisterGeneric(typeof(ProcessorEngine<>))
                .AsSelf();
        }

        public class FlowFileBatchControllerFactory : IFlowFileBatchControllerFactory
        {
            private readonly IComponentContext _context;

            public FlowFileBatchControllerFactory(IComponentContext context)
            {
                this._context = context;
            }

            public IFlowFileBatchController Create(Type type)
            {
                // ReSharper disable once UsePatternMatching
                var batchProcessor = _context.Resolve(type) as IFlowFileBatchController;
                if (batchProcessor == null)
                    throw new InvalidOperationException($"Controller type does not implement {typeof(IFlowFileBatchController).Name}");

                return batchProcessor;
            }
        }

        public class FlowFileLogRepositoryFactory : IFlowFileLogRepositoryFactory
        {
            private readonly IComponentContext _context;

            public FlowFileLogRepositoryFactory(IComponentContext context)
            {
                this._context = context;
            }

            public FlowFileLogRepository Get()
            {
                return _context.Resolve<FlowFileLogRepository>();
            }
        }

        public class FlowFileControllerServiceFactory:  IFlowFileControllerServiceFactory
        {
            private readonly IComponentContext _context;

            public FlowFileControllerServiceFactory(IComponentContext context)
            {
                this._context = context;
            }

            public FlowFileControllerService Get(IFlowFileLogRepository repository, IFlowFileBatchController controller)
            {
                return _context.Resolve<FlowFileControllerService>(
                    new TypedParameter(typeof(IFlowFileLogRepository), repository),
                    new TypedParameter(typeof(IFlowFileBatchController), controller));
            }
        }
    }
}