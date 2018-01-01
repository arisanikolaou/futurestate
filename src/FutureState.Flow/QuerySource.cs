﻿using FutureState.Flow.Data;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FutureState.Flow
{

    /// <summary>
    ///     A prospective data source for any given processor that is capable of
    /// playing back messages to a consumer from any given point of time represented by a 'check point'.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of entity being queried.
    /// </typeparam>
    public class QuerySource<TEntity>
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
        /// <param name="flowId">The associated flow id.</param>
        /// <param name="dataProvider">Local id and the page size.</param>
        public QuerySource(Guid flowId, Func<int, int, QueryResponse<TEntity>> dataProvider)
        {
            FlowId = flowId;

            _receiveFn = dataProvider;
            _repository = new QueryResponseStateRepository(Environment.CurrentDirectory, typeof(TEntity));
        }

        /// <summary>
        ///     Creates a new package for a given flow, consumer id.
        /// </summary>
        /// <remarks>
        ///     A consumer id must be unique for a given 'Flow'.
        /// </remarks>
        /// <param name="processorId">
        ///     The id of the consumer requesting the snapshot data (the package).
        /// </param>
        /// <param name="sequenceFrom">
        ///     Used to map the starting point to playback messages to the
        /// consumer.
        /// </param>
        /// <param name="pageSize">
        ///     The window size to assemble a snapshot for.
        /// </param>
        /// <returns></returns>
        public virtual QueryResponse<TEntity> Get(
            string processorId,
            Guid sequenceFrom,
            int pageSize)
        {
            List<QueryResponseState> state = _repository.Get(processorId);

            try
            {
                // find the internal row id (or line id)
                int localId = state.Where(m => m.CheckPoint == sequenceFrom).Select(m => m.LocalIndex).FirstOrDefault();

                // get query response
                QueryResponse<TEntity> response =  _receiveFn(localId, pageSize);

                response.CheckPointFrom = sequenceFrom;
                response.Package.FlowId = FlowId;

                if (response == null)
                    throw new InvalidOperationException("Response was not received.");

                state.Add(new QueryResponseState()
                {
                    FlowId = this.FlowId,
                    ProcessorId = processorId,
                    LocalIndex = response.CheckPointLocalTo,
                    CheckPoint = response.CheckPointTo
                });

                return response;
            }
            finally
            {
                // save query response and local check point to map external checkpoint to an internal one
                _repository.Save(processorId, state);
            }
        }
    }
}