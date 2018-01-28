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

            cb.RegisterType<FlowFileLogRepository>().AsSelf().AsImplementedInterfaces();

            cb.RegisterType<FlowFileProcessorService>().AsSelf().AsImplementedInterfaces();

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
    }
}