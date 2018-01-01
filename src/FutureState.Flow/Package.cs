using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class Package : Package<object>
    {
    }

    /// <summary>
    ///     A batch/package of entites exchanged between a query source and a processor.
    /// </summary>
    /// <typeparam name="TEntity">The batch entity type.</typeparam>
    public class Package<TEntity>
    {
        /// <summary>
        ///     The flow identifier.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        ///     The part or step in the package.
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        ///     The display name of the flow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The underlying package data.
        /// </summary>
        public List<TEntity> Data { get; set; }

        /// <summary>
        ///     Gets the errors encountered processing data.
        /// </summary>
        public List<ProcessError> Errors { get; set; }

        /// <summary>
        ///     Invalid entities.
        /// </summary>
        public List<ProcessEntityError> Invalid { get; set; }
    }
}