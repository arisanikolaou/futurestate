using System;
using System.Collections.Generic;
using System.Linq;
using FutureState.Flow.Data;

namespace FutureState.Flow.QuerySources
{
    public class QuerySourceFromPackage<TEntity> : QuerySource<TEntity>
    {
        public QuerySourceFromPackage(Guid flowId, PackageRepository<TEntity> repository)
            : base(flowId, GetFlowFn(flowId, repository))
        {
        }

        private static Func<int, int, QueryResponse<TEntity>> GetFlowFn(Guid flowId,
            PackageRepository<TEntity> repository)
        {
            var entities = repository
                .GetEntities<TEntity>()
                .ToList();

            return (checkPointLocal, pageSize) =>
            {
                int localIndex;

                var outPut = new List<TEntity>();

                // package only the entities requested
                for (localIndex = checkPointLocal; localIndex < pageSize && localIndex < entities.Count; localIndex++)
                    outPut.Add(entities[localIndex]);

                // create response package
                var package = new Package<TEntity>(flowId)
                {
                    Data = outPut
                };

                return new QueryResponse<TEntity>(package, localIndex);
            };
        }
    }
}