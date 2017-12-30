using FutureState.Data;
using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A response to a give receive request.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The entity type to materialize from the underlying source.
    /// </typeparam>
    public class QueryResponse<TEntity>
    {
        /// <summary>
        ///     Gets the start of the snapshot.
        /// </summary>
        public Guid SequenceFrom { get; set; }

        /// <summary>
        ///     Gets the id of the end of the snapshot. This will be a sequential id.
        /// </summary>
        public Guid SequenceTo { get; set; }


        /// <summary>
        ///     Gets/sets the local id.
        /// </summary>
        public int LocalId { get; set; }

        /// <summary>
        ///     Gets the package that was assembled.
        /// </summary>
        public Package<TEntity> Package { get; set; }


        public QueryResponse()
        {

        }


        public QueryResponse(Package<TEntity> package, int localId)
        {
            Package = package;
            LocalId = localId;
            SequenceTo = SeqGuid.Create(); // next check point
        }
    }
}