using FutureState.Flow.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.QuerySources
{
    public class QuerySourceFromPackage<TEntity> : QuerySource<TEntity>
    {
        public QuerySourceFromPackage(Guid flowId, PackageRepository<TEntity> repository)
           : base(flowId, GetFlowFn(flowId, repository))
        {

        }

        static Func<int, int, QueryResponse<TEntity>> GetFlowFn(Guid flowId, PackageRepository<TEntity> repository)
        {
            var entities = repository.GetEntities<TEntity>().ToList();

            return (checkPointLocal, pageSize) =>
            {
                int localIndex = checkPointLocal;

                var outPut = new List<TEntity>();

                // package only the entities requested
                for (localIndex = checkPointLocal; localIndex < pageSize && localIndex < entities.Count; localIndex++)
                    outPut.Add(entities[localIndex]);

                // create package to feed processor
                var package = new Package<TEntity>()
                {
                    FlowId = flowId,
                    Data = entities
                };

                return new QueryResponse<TEntity>(package, localIndex);
            };
        }
    }
}
