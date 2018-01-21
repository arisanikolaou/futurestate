using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     Gets the query response transaction record (state).
    /// </summary>
    public class QueryResponseState
    {
        /// <summary>
        ///     Gets the processor id querying the data.
        /// </summary>
        public string ProcessorId { get; set; }

        /// <summary>
        ///     Gets the local index being read.
        /// </summary>
        public int LocalIndex { get; set; }

        /// <summary>
        ///     Gets the consumer scoped checkpoint id for the local index.
        /// </summary>
        public Guid CheckPoint { get; set; }

        /// <summary>
        ///     The flow id asssociated with the response.
        /// </summary>
        public Guid FlowId { get; set; }
    }
}