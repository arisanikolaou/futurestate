using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class FlowPackage : FlowPackage<object>
    {
    }

    /// <summary>
    ///     A batch/flowPackage of entites exchanged between a query source and a processor.
    /// </summary>
    /// <typeparam name="TEntity">The batch entity type.</typeparam>
    public class FlowPackage<TEntity>
    {
        public FlowPackage()
        {
        }

        public FlowPackage(Guid flowId)
        {
            FlowId = flowId;

            Invalid = new List<FlowProcessError>();
            Data = new List<TEntity>();
        }

        /// <summary>
        ///     The flow identifier.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        ///     The part or step in the flowPackage.
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        ///     The underlying flowPackage data.
        /// </summary>
        public List<TEntity> Data { get; set; }

        /// <summary>
        ///     Invalid entities.
        /// </summary>
        public List<FlowProcessError> Invalid { get; set; }
    }
}