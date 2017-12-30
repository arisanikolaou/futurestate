using FutureState.Flow.Data;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class QuerySource : PortSource<object>
    {
        public QuerySource(Func<int, int, QueryResponse<object>> receiveFn) : base(receiveFn)
        {
        }
    }

    // the data source for a given processsor. this is essentially a data stream connection

    /// <summary>
    ///     A prospective data source for any given processor that is capable of
    /// playing back messages to a consumer from any given point of time.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class PortSource<TEntity>
    {
        private static readonly object _syncLock = new object();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Func<int, int, QueryResponse<TEntity>> _receiveFn;

        public QueryResponseStateRepository _repository { get; }

        /// <summary>
        ///     Gets the correlation id.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        ///     Gets the port source unique id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="receiveFn"></param>
        public PortSource(Func<int, int, QueryResponse<TEntity>> receiveFn)
        {
            _receiveFn = receiveFn;
            _repository = new QueryResponseStateRepository(Environment.CurrentDirectory, typeof(TEntity));
        }

        /// <summary>
        ///     Creates a new package for a given flow, consumer id.
        /// </summary>
        /// <remarks>
        ///     A consumer id must be unique for a given 'Flow'.
        /// </remarks>
        /// <param name="consumerId">
        ///     The id of the consumer requesting the snapshot data (the package).
        /// </param>
        /// <param name="sequenceFrom">
        ///     Used to map the starting point to playback messages to the
        /// consumer.
        /// </param>
        /// <param name="entitiesCount">
        ///     The window size to assemble a snapshot for.
        /// </param>
        /// <returns></returns>
        public virtual QueryResponse<TEntity> Get(
            string consumerId,
            Guid sequenceFrom,
            int entitiesCount)
        {
            List<QueryResponseState> state = _repository.Get(consumerId);

            try
            {
                // find the internal row id (or line id)
                int localId = state.Where(m => m.CheckPoint == sequenceFrom).Select(m => m.LocalIndex).FirstOrDefault();

                QueryResponse<TEntity> response =  _receiveFn(localId, entitiesCount);

                response.SequenceFrom = sequenceFrom;
                response.Package.FlowId = FlowId;

                if (response == null)
                    throw new InvalidOperationException("Response was not received.");

                state.Add(new QueryResponseState()
                {
                    FlowId = this.FlowId,
                    ConsumerId = consumerId,
                    LocalIndex = response.LocalId,
                    CheckPoint = response.SequenceTo
                });

                return response;
            }
            finally
            {
                // save query response and local check point to map external checkpoint to an internal one
                _repository.Save(consumerId, state);
            }
        }
    }
}