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
        public Guid CheckPointFrom { get; set; }

        /// <summary>
        ///     Gets the id of the end of the snapshot. This will be a sequential id.
        /// </summary>
        public Guid CheckPointTo { get; set; }

        /// <summary>
        ///     Gets/sets the local id.
        /// </summary>
        public int LocalId { get; set; }

        /// <summary>
        ///     Gets the package that was assembled to satisfy a request.
        /// </summary>
        public Package<TEntity> Package { get; set; }


        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public QueryResponse()
        {
            // will be required for serializer
        }


        public QueryResponse(Package<TEntity> package, int localId)
        {
            Package = package;
            LocalId = localId;
            CheckPointTo = SeqGuid.Create(); // next check point
        }

        public QueryResponse(Guid checkPointFrom, Guid checkPointTo, int localId, Package<TEntity> package)
        {
            CheckPointFrom = checkPointFrom;
            CheckPointTo = checkPointTo;
            LocalId = localId;
            Package = package;
        }
    }
}