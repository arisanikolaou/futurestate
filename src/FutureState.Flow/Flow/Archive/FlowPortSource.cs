using System;

namespace FutureState.Flow
{
    public class FlowPortSource : FlowPortSource<object>
    {
        public FlowPortSource(Func<string, int, Guid, ReceiveMessageResponse<object>> receiveFn) : base(receiveFn)
        {
        }
    }

    // the data source for a given processsor. this is essentially a data stream connection
    // this could be a csv reader

    /// <summary>
    ///     A prospective data source for any given processor that is capable of
    ///     playing back messages to a consumer from any given point of time.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class FlowPortSource<TEntity>
    {
        private readonly Func<string, int, Guid, ReceiveMessageResponse<TEntity>> _receiveFn;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="receiveFn"></param>
        public FlowPortSource(Func<string, int, Guid, ReceiveMessageResponse<TEntity>> receiveFn)
        {
            _receiveFn = receiveFn;
        }

        /// <summary>
        ///     Gets the process id.
        /// </summary>
        public Guid ProcessorId { get; set; }

        /// <summary>
        ///     Gets the data source description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Creates a new flowPackage for a given flow, consumer id.
        /// </summary>
        /// <remarks>
        ///     A consumer id must be unique for a given 'Flow'.
        /// </remarks>
        /// <param name="consumerId">
        ///     The id of the consumer requesting the BatchProcess data (the flowPackage).
        /// </param>
        /// <param name="sequenceFrom">
        ///     Used to map the starting point to playback messages to the
        ///     consumer.
        /// </param>
        /// <param name="entitiesCount">
        ///     The window size to assemble a BatchProcess for.
        /// </param>
        /// <returns></returns>
        public virtual ReceiveMessageResponse<TEntity> Receive(
            string consumerId,
            Guid sequenceFrom,
            int entitiesCount)
        {
            return _receiveFn(consumerId, entitiesCount, sequenceFrom);
        }
    }

    /// <summary>
    ///     A response to a give receive request.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The entity type to materialize from the underlying source.
    /// </typeparam>
    public class ReceiveMessageResponse<TEntity>
    {
        /// <summary>
        ///     Gets the start of the BatchProcess.
        /// </summary>
        public Guid SequenceFrom { get; set; }

        /// <summary>
        ///     Gets the id of the end of the BatchProcess. This will be a sequential id.
        /// </summary>
        public Guid SequenceTo { get; set; }

        /// <summary>
        ///     Gets the flowPackage that was assembled.
        /// </summary>
        public FlowPackage<TEntity> FlowPackage { get; set; }
    }
}