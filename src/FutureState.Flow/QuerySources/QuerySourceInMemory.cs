using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.QuerySources
{
    public class QuerySourceInMemory<TEntity> : QuerySource<TEntity>
    {
        public QuerySourceInMemory(Guid flowId, IEnumerable<TEntity> entities)
            : base(flowId, GetFlowFn(flowId, entities.ToList()))
        {
        }

        private static Func<int, int, QueryResponse<TEntity>> GetFlowFn(Guid flowId, List<TEntity> entities)
        {
            return (checkPointLocal, pageSize) =>
            {
                int localIndex;

                var outPut = new List<TEntity>();

                // package only the entities requested
                for (localIndex = checkPointLocal; localIndex < pageSize && localIndex < entities.Count; localIndex++)
                    outPut.Add(entities[localIndex]);

                var package = new Package<TEntity>(flowId)
                {
                    Data = outPut
                };

                return new QueryResponse<TEntity>(package, localIndex);
            };
        }
    }
}