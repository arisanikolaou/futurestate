using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using System;

namespace FutureState.Flow
{

    public interface IFlowFileControllerFactory
    {
        IFlowFileController Create(Type type);
    }

    public interface IFlowFileLogRepositoryFactory
    {
        FlowFileLogRepo Get();
    }

    public interface IFlowFileControllerServiceFactory
    {
        FlowFileControllerService Get(IFlowFileLogRepo repository, IFlowFileController controller);
    }
}
