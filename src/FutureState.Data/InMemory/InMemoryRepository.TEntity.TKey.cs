using EmitMapper;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FutureState.Data
{
    /// <summary>
    ///     An in memory repository for a small set of objects.
    /// </summary>
    /// <typeparam name="TEntity">The entity to store.</typeparam>
    /// <typeparam name="TKey">The entity's primary key type.</typeparam>
    public class InMemoryRepository<TEntity, TKey> :
        IRepositoryLinq<TEntity, TKey>,
        IBulkLinqReader<TEntity, TKey>,
        IBulkRepository<TEntity, TKey>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IKeyProvider<TEntity, TKey> _keyProvider;

        private readonly ConcurrentDictionary<TKey, TEntity> _items;

        private readonly IKeyBinder<TEntity, TKey> _keyBinder;

        private readonly ObjectMapperManager _mapper;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="keyProvider">Required. A function to return the value of the entity's primary key.</param>
        /// <param name="keyBinder">The binder of the entity's id.</param>
        /// <param name="items">Required. The list of entities to pre-populate the current instance with.</param>
        /// <param name="mapper">Object mapper manager instance</param>
        public InMemoryRepository(
            IKeyProvider<TEntity, TKey> keyProvider,
            IKeyBinder<TEntity, TKey> keyBinder,
            IEnumerable<TEntity> items,
            ObjectMapperManager mapper)
        {
            Guard.ArgumentNotNull(keyProvider, nameof(keyProvider));
            Guard.ArgumentNotNull(keyBinder, nameof(keyBinder));
            Guard.ArgumentNotNull(items, nameof(items));
            Guard.ArgumentNotNull(mapper, nameof(mapper));

            _keyProvider = keyProvider;
            _keyBinder = keyBinder;
            _mapper = mapper;
            _items =
                new ConcurrentDictionary<TKey, TEntity>(
                    items.Select(i => new KeyValuePair<TKey, TEntity>(_keyBinder.Get(i), i)));
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="keyProvider">Required. A function to return the value of the entity's primary key.</param>
        /// <param name="keyBinder">The binder for the entity id.</param>
        /// <param name="items">Required. The list of entities to pre-populate the current instance with.</param>
        public InMemoryRepository(
            IKeyProvider<TEntity, TKey> keyProvider,
            IKeyBinder<TEntity, TKey> keyBinder,
            IEnumerable<TEntity> items)
            : this(keyProvider, keyBinder, items, ObjectMapperManager.DefaultInstance)
        {
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="keyProvider">Required. A function to return the value of the entity's primary key.</param>
        /// <param name="keyBinder">The binder for the entity id.</param>
        public InMemoryRepository(
            IKeyProvider<TEntity, TKey> keyProvider,
            IKeyBinder<TEntity, TKey> keyBinder)
            : this(keyProvider, keyBinder, Array.Empty<TEntity>(), ObjectMapperManager.DefaultInstance)
        {
        }

        /// <summary>
        ///     Creates a new instance using AssignedGenerator and ExpressionKeyBinder.
        /// </summary>
        /// <param name="getKey">Required. A function to return the value of the entity's primary key.</param>
        /// <param name="items">default items collection</param>
        public InMemoryRepository(Func<TEntity, TKey> getKey, IEnumerable<TEntity> items)
            : this(
                new KeyProviderNoOp<TEntity, TKey>(),
                new KeyBinder<TEntity, TKey>(getKey, (_, __) => { }),
                items)
        {
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="getKey">Required. A function to return the value of the entity's primary key.</param>
        public InMemoryRepository(Func<TEntity, TKey> getKey)
            : this(getKey, Enumerable.Empty<TEntity>())
        {
        }

        /// <summary>
        ///     Creates a new instance for all entities that support the IEntity interface.
        /// </summary>
        public InMemoryRepository()
            : this(GetGetIdFunc(), Enumerable.Empty<TEntity>())
        {
        }

        //see interface documentation
        public IEnumerable<TEntity> GetTopByKeys<TQueryArg>(
            IEnumerable<TQueryArg> queryArgs,
            Expression<Func<TEntity, TQueryArg, bool>> matchExpression,
            Expression<Func<TEntity, object>> maxEntityColumnKeyExpression,
            Expression<Func<TEntity, bool>> whereExpression,
            Expression<Func<TEntity, object>> orderByExpression)
        {
            var match = matchExpression.Compile();

            var unionResultFilteredAndOrdered = _items.Values
                .Where(whereExpression.Compile()) //filter out any non match results by custom where
                .Where(
                    e => queryArgs.Any(d => match(e, d)))
                //order top by
                .OrderByDescending(maxEntityColumnKeyExpression.Compile());

            //list is already ordered
            var topDistinctEntities = new List<TEntity>();

            //an - no doubt this can be optimized
            foreach (var entity in unionResultFilteredAndOrdered)
            {
                //find query arg that matches the result
                var queryArg = queryArgs.FirstOrDefault(m => match(entity, m));

                //queryArg will never be null

                if (!topDistinctEntities.Any(existing => match(existing, queryArg)))
                    topDistinctEntities.Add(entity);
            }

            return topDistinctEntities
                .OrderBy(orderByExpression.Compile());
        }

        public void DeleteByIds(IEnumerable<TKey> ids)
        {
            // ReSharper disable once RedundantAssignment
            var entity = default(TEntity);

            foreach (var id in ids)
                _items.TryRemove(id, out entity);
        }

        public void SaveOrUpdate(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                var key = _keyBinder.Get(entity);

                _items.AddOrUpdate(key, entity, (key1, entity1) => entity);
            }
        }

        // ReSharper disable once IdentifierTypo

        /// <summary>
        ///     Inserts the specified entity into the underlying repository.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.InvalidOperationException">Specified entity already exists.</exception>
        public void Insert(TEntity entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            _keyProvider.Provide(entity);

            var key = _keyBinder.Get(entity);

            if (!_items.TryAdd(key, entity))
                throw new InvalidOperationException($"An entity already exists with the key {key}.");
        }

        public void Update(TEntity entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            var key = _keyBinder.Get(entity);
            TEntity existing;

            if (!_items.TryGetValue(key, out existing))
                throw new ArgumentOutOfRangeException(
                    nameof(entity),
                    $"Item with the key {key} doesn't exist.");

            if (!_items.TryUpdate(key, entity, existing))
                throw new InvalidOperationException("Unable to update the entity.");
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            Guard.ArgumentNotNull(entities, "items");

            foreach (var item in entities)
                Update(item);
        }

        public void DeleteById(TKey key)
        {
            if (!_items.TryRemove(key, out _))
                _logger.Trace(@"Item {0} has been deleted or does not exist.", key);
        }

        public void Delete(TEntity entity)
        {
            DeleteById(_keyBinder.Get(entity));
        }

        public void DeleteAll()
        {
            _items.Clear();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _items.Values;
        }

        // private to support method inlining in release mode when called internally
        public TEntity Get(TKey key)
        {
            TEntity existing;

            if (!_items.TryGetValue(key, out existing))
                _logger.Trace("Item with key {0} doesn't exist.", key);

            return existing;
        }

        public IEnumerable<TEntity> GetByIds(IEnumerable<TKey> ids)
        {
            return _items.Where(m => ids.Contains(m.Key)).Select(m => m.Value);
        }

        public IEnumerable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            return GetAllInner().Where(predicate.Compile());
        }

        /// <summary>
        ///     Finds a set of entities based on a given set of descriptors and an expression.
        /// </summary>
        /// <typeparam name="TQueryArg">
        ///     A descriptor type which must be decorated with an Alias attribute: [Alias("#descriptors")]
        ///     Internally, this type is defined as a table, thus it must define a primary key,
        ///     the Id property with the AutoIncrement attriubte can be used in this occasion.
        ///     Preferably, it should define a composite unique key for faster performance.
        /// </typeparam>
        /// <param name="queryArgs">A set of descriptors.</param>
        /// <param name="matchExpression">An expression on how to use descriptors.</param>
        /// <returns>A list of entities.</returns>
        public IEnumerable<TEntity> GetByKeys<TQueryArg>(IEnumerable<TQueryArg> queryArgs,
            Expression<Func<TEntity, TQueryArg, bool>> matchExpression)
        {
            var filter = matchExpression.Compile();
            return _items.Values.Where(e => queryArgs.Any(d => filter(e, d)));
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            return Where(predicate).Any();
        }

        public bool Any()
        {
            return _items.Count > 0;
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            return GetAllInner().SingleOrDefault(predicate.Compile());
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            return GetAllInner().FirstOrDefault(predicate.Compile());
        }

        public IEnumerable<TProjection> Select<TProjection>(Expression<Func<TEntity, bool>> predicate)
            where TProjection : new()
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            Func<TEntity, TProjection> map = _mapper.GetMapper<TEntity, TProjection>().Map;

            return GetAllInner()
                .Where(predicate.Compile())
                .Select(map);
        }

        public IEnumerable<TProjection> Select<TProjection>()
            where TProjection : new()
        {
            Func<TEntity, TProjection> map = _mapper.GetMapper<TEntity, TProjection>().Map;

            return GetAllInner()
                .Select(map);
        }

        /// <summary>
        ///     Gets the number of items in the collection.
        /// </summary>
        public long Count()
        {
            return _items.Count;
        }

        /// <summary>
        ///     Gets the number of objects in the underlying collection matching the given predicate.
        /// </summary>
        public long Count(Expression<Func<TEntity, bool>> predicate)
        {
            return Where(predicate).Count();
        }

        /// <summary>
        ///     Inserts a batch of entities into the underlying repository.
        /// </summary>
        /// <param name="entities">The batch of entities to add.</param>
        public void Insert(IEnumerable<TEntity> entities)
        {
            Guard.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                _keyProvider.Provide(entity);

                var key = _keyBinder.Get(entity);

                if (!_items.TryAdd(key, entity))
                    throw new InvalidOperationException($"Specified entity '{entity}' already exists.");
            }
        }

        public PageResponse<TEntity> Get(Action<IPageRequest<TEntity>> key)
        {
            Guard.ArgumentNotNull(key, nameof(key));

            var request = new PageRequest();

            key?.Invoke(request);

            var items = request.Process(_items.Values);

            return new PageResponse<TEntity>(items, _items.Count);
        }

        /// <summary>
        ///     Deletes a set of records by a given predicate.
        /// </summary>
        /// <param name="predicate">An expression on how to delete.</param>
        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            Where(predicate).Each(Delete);
        }

        /// <summary>
        ///     Resets the repository by clearing all underlying items.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        public static Func<TEntity, TKey> GetGetIdFunc()
        {
            if (typeof(IEntity<TKey>).IsAssignableFrom(typeof(TEntity)))
                return item => ((IEntity<TKey>)item).Id;

            throw new NotSupportedException(
                $"Entity {typeof(TEntity).Name} is not supported by the default constructor of 'InMemoryRepository' as it does not implement {typeof(IEntity<>).Name}.");
        }

        private IEnumerable<TEntity> GetAllInner()
        {
            return _items.Values;
        }

        #region PageRequest

        private class PageRequest : IPageRequest<TEntity>
        {
            private Func<TEntity, bool> _filter;

            private Func<IEnumerable<TEntity>, IOrderedEnumerable<TEntity>> _order;

            private int _page;

            private int _size;

            public PageRequest()
            {
                _page = 1;
                _size = 1; // set size to one by default?
                _filter = _ => true;
            }

            public IPageRequest<TEntity> Asc(Expression<Func<TEntity, object>> sortExpression)
            {
                Guard.ArgumentNotNull(sortExpression, nameof(sortExpression));
                _order = source => source.OrderBy(sortExpression.Compile());
                return this;
            }

            public IPageRequest<TEntity> Desc(Expression<Func<TEntity, object>> sortExpression)
            {
                Guard.ArgumentNotNull(sortExpression, nameof(sortExpression));
                _order = source => source.OrderByDescending(sortExpression.Compile());
                return this;
            }

            public IPageRequest<TEntity> SetFilter(Expression<Func<TEntity, bool>> filterExpression)
            {
                Guard.ArgumentNotNull(filterExpression, nameof(filterExpression));
                _filter = filterExpression.Compile();
                return this;
            }

            public IPageRequest<TEntity> SetPageNumber(int page)
            {
                if (page < 1)
                    throw new ArgumentException("pages are counted from 1", nameof(page));

                _page = page;
                return this;
            }

            public IPageRequest<TEntity> SetPageSize(int size)
            {
                if (size < 1)
                    throw new ArgumentException("size should be at least 1", nameof(size));

                _size = size;
                return this;
            }

            public IEnumerable<TEntity> Process(IEnumerable<TEntity> source)
            {
                if (_order == null)
                    throw new InvalidOperationException("Order has to be set.");

                return _order(source.Where(_filter))
                    .Skip(_size * (_page - 1))
                    .Take(_size);
            }
        }

        #endregion PageRequest
    }
}