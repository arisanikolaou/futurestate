using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow
{
    /// <summary>
    ///     Service to create/remove flows and flow batches.
    /// </summary>
    public class FlowService : IFlowService
    {
        private readonly FlowIdRepo _repo;

        static readonly object _syncLock = new object();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="repo">The repository of flows.</param>
        public FlowService(FlowIdRepo repo)
        {
            Guard.ArgumentNotNull(repo, nameof(repo));

            _repo = repo;
        }

        /// <summary>
        ///     Creates a new flow flow.
        /// </summary>
        /// <param name="flowCode">The unique flow code.</param>
        /// <returns></returns>
        public FlowId CreateNew(string flowCode)
        {
            var flow = new FlowId()
            {
                Code = flowCode,
                DataDir = Environment.CurrentDirectory,
                DisplayName = flowCode,
                Entities = new List<FlowEntity>(),
                CurrentBatchId = 1
            };

            Save(flow);

            return flow;
        }

        /// <summary>
        ///     Gets a flow by its code.
        /// </summary>
        public FlowId Get(string flowCode)
        {
            return _repo.Get(flowCode);
        }

        /// <summary>
        ///     Saves a flow.
        /// </summary>
        /// <param name="flow">The flow to save.</param>
        public void Save(FlowId flow)
        {
            if (_repo.Exists(flow.Code))
                throw new InvalidOperationException($"Another flow with the code {flow.Code} already exists.");

            _repo.Save(flow);
        }

        /// <summary>
        ///     Registers an entity against the flow.
        /// </summary>
        /// <typeparam name="TEntityType">The entity type to create.</typeparam>
        /// <param name="flowCode">The flowto save the registered entity type against.</param>
        /// <returns></returns>
        public FlowEntity RegisterEntity<TEntityType>(string flowCode)
        {
            var flow = Get(flowCode);

            string entityTypeId = typeof(TEntityType).Name;

            if (flow.Entities.Any(m => string.Equals(m.EntityTypeId, entityTypeId)))
                throw new InvalidOperationException($"The entity type has already been registered.");

            var entity = new FlowEntity()
            {
                AssemblyQualifiedTypeName = typeof(TEntityType).AssemblyQualifiedName,
                DateAdded = DateTime.UtcNow,
                EntityTypeId = entityTypeId
            };

            // add to collection
            flow.Entities.Add(entity);

            // update
            _repo.Save(flow);

            //
            return entity;
        }

        /// <summary>
        ///     Registers and entity with a given flow.
        /// </summary>
        public void RegisterEntity(string flowCode, FlowEntity entity)
        {
            var flow = Get(flowCode);

            if (flow.Entities.Any(m => string.Equals(m.EntityTypeId, entity.EntityTypeId)))
                throw new InvalidOperationException($"The entity type has already been registered.");

            flow.Entities.Add(entity);

            // record data added
            entity.DateAdded = DateTime.UtcNow;

            // update
            _repo.Save(flow);
        }

        /// <summary>
        ///     Creates a new unique flow batch record against the flow.
        /// </summary>
        public FlowBatch GetNewFlowBatch(string flowCode)
        {
            var flow = Get(flowCode);

            lock (_syncLock)
            {
                if (flow == null)
                    flow = new FlowId(flowCode); // just create new flow code

                var newBatchId = flow.CurrentBatchId++;

                var batch = new FlowBatch()
                {
                    Flow = flow,
                    BatchId = newBatchId,
                };

                // save/update original file - don't call save as it will through error
                _repo.Save(flow);

                // now return to caller
                return batch;
            }
        }
    }
}