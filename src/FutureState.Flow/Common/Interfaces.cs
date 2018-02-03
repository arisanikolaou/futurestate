using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureState.Flow
{

    public interface IFlowFileControllerFactory
    {
        IFlowFileController Create(Type type);
    }

    public interface IFlowFileLogRepositoryFactory
    {
        FlowFileLogRepository Get();
    }

    public interface IFlowFileControllerServiceFactory
    {
        FlowFileControllerService Get(IFlowFileLogRepository repository, IFlowFileController controller);
    }
}
