#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState.IO
{
    /// <summary>
    /// Splits a set into equal weighted chunks/buckets or batches.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BatchingEnumerator<TEntity> : IBatchingEnumerator<TEntity>
    {
        private readonly IEnumerable<TEntity> _items;

        private IList<TEntity> _current;

        private int _currentPage;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="batchSize">Size if  each batch which must be greater than one.</param>
        public BatchingEnumerator(IEnumerable<TEntity> items, int batchSize = 1000)
        {
            Guard.ArgumentNotNull(items, nameof(items));
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), @"Parameter 'batchSize' must be greater than zero.");
            }

            _items = items;
            BatchSize = batchSize;

            CalcCurrent(0);
        }

        /// <summary>
        /// The configured batch size.
        /// </summary>
        public int BatchSize { get; }

        public IEnumerable<TEntity> GetCurrentItems()
        {
            return _current;
        }

        public bool MoveNext()
        {
            CalcCurrent(_currentPage++);

            return _current.Any();
        }

        private IEnumerable<T> Batch<T>(IEnumerable<T> source, int size, int page)
        {
            source = source.Skip(page * size);

            var itemsCount = source.Count();

            var sourceArray = source.ToArray();

            for (var i = 0; i < itemsCount && i < size; i++)
            {
                yield return sourceArray[i];
            }
        }

        private void CalcCurrent(int page)
        {
            _current = Batch(_items, BatchSize, page).ToList();
        }
    }

    public class BatchingEnumerator<TEntity, TKey> : IBatchingEnumerator<TEntity>
    {
        private readonly Func<IEnumerable<TKey>, IEnumerable<TEntity>> _getEntities;

        private readonly List<IEnumerable<TKey>> _slices;

        private List<TEntity> _current;

        private int _currentSlice;

        public BatchingEnumerator(IEnumerable<TKey> keys, Func<IEnumerable<TKey>, IEnumerable<TEntity>> getEntities,
            int batchSize = 1000)
        {
            Guard.ArgumentNotNull(keys, nameof(keys));
            Guard.ArgumentNotNull(getEntities, nameof(getEntities));
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), @"Parameter 'batchSize' must be greater than zero.");
            }

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