using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using System;

namespace FutureState.Flow
{

    public interface IFlowFileControllerFactory
    {
        /// <summary>
        ///     Creates a flow file controller from a given controller type.
        /// </summary>
        /// <param name="type">An instance of IFlowFileController.</param>
        /// <returns></returns>
        IFlowFileController Create(Type type);
    }

    public interface IFlowFileLogRepositoryFactory
    {
        /// <summary>
        ///     Gets/creates the flow snapshot log repo.
        /// </summary>
        /// <returns></returns>
        FlowFileLogRepo Get();
    }

    public interface IFlowFileControllerServiceFactory
    {
        FlowFileControllerService Get(IFlowFileLogRepo repository, IFlowFileController controller);
    }
}
