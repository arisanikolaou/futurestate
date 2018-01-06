#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState.IO
{
    /// <summary>
    ///     Implements batching enumerator to query sets of keyed entities using a function callback.
    /// </summary>
    /// <typeparam name="TEntity">The entity to query for.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    public class BatchingEnumeratorKeyedQuery<TEntity, TKey> : IBatchingEnumerator<TEntity>
    {
        private readonly Func<IEnumerable<TKey>, IEnumerable<TEntity>> _getEntities;

        private readonly List<IEnumerable<TKey>> _slices;

        private List<TEntity> _current;

        private int _currentSlice;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="keys">The full set of keys to query for..</param>
        /// <param name="getEntities">Function to get entities by their ids.</param>
        /// <param name="batchSize">The batch size to split the results. The default is 1000.</param>
        public BatchingEnumeratorKeyedQuery(IEnumerable<TKey> keys,
            Func<IEnumerable<TKey>, IEnumerable<TEntity>> getEntities, int batchSize = 1000)
        {
            Guard.ArgumentNotNull(keys, nameof(keys));
            Guard.ArgumentNotNull(getEntities, nameof(getEntities));
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize),
                    @"Parameter 'batchSize' must be greater than zero.");

            _slices = keys.Slice(batchSize).ToList();
            _currentSlice = 0;
            _getEntities = getEntities;
            _current = new List<TEntity>();

            BatchSize = batchSize;
        }

        public int BatchSize { get; }

        public IEnumerable<TEntity> GetCurrentItems()
        {
            return _current;
        }

        public bool MoveNext()
        {
            if (_currentSlice < _slices.Count)
            {
                _current = _getEntities(_slices[_currentSlice]).ToList();
                _currentSlice++;
                return true;
            }

            _current.Clear();

            return false;
        }
    }
}