using FutureState.Services;

namespace FutureState.Flow
{
    public interface IFlowService : IService
    {
        FlowId CreateNew(string flowCode);
        FlowId Get(string flowCode);
        FlowBatch GetNewFlowBatch(string flowCode);
        void RegisterEntity(string flowCode, FlowEntity entity);
        FlowEntity RegisterEntity<TEntityType>(string flowCode);
        void Save(FlowId flow);
    }
}