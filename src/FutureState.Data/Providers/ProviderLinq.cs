using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Services;
using FutureState.Specifications;
using NLog;

namespace FutureState.Data.Providers
{
    /// <summary>
    ///     A generic basic query used to query, add/remove or update entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity to persist.</typeparam>
    /// <typeparam name="TKey">The entity's primary key.</typeparam>
    public class ProviderLinq<TEntity, TKey> : IService
        where TEntity : class, IEntityMutableKey<TKey>
        where TKey : IEquatable<TKey>
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //default specs used to validate keys
#pragma warning disable RECS0108 // Warns about static fields in generic types
        private static readonly IList<ISpecification<TEntity>> _defaultSpecs;

        private readonly IList<ISpecification<TEntity>> _specs;

        private readonly IEntityIdProvider<TEntity, TKey> _idProvider;

        // processes crud actions
        private readonly Stack<Action<TKey>> _onBeforeDelete;

        private readonly Stack<Action<TEntity>> _onBeforeInsert;
        private readonly Stack<Action<TEntity>> _onInitialize;

        // where to dispatch domain events

        /// <summary>
        ///     Gets the message pipe used by the current instance to communicate domain events.
        /// </summary>
        public IMessagePipe MessagePipe { get; }

        static ProviderLinq()
        {
            // default service specs based on data annotations
            _defaultSpecs = new DataAnnotationsSpecProvider<TEntity>().GetSpecifications().ToList();
        }

        protected internal IUnitOfWorkLinq<TEntity, TKey> Db { get; }

        /// <summary>
        ///     Creates a new generic service to add/update valid entities and/or remove them from the application.
        /// </summary>
        /// <param name="messagePipe">Message pipe to dispatch domain events.</param>
        /// <param name="db">The data store to use to persist the entity.</param>
        /// <param name="idProvider">Function to get a new key for a given entity.</param>
        /// <param name="specProvider">
        ///     The custom rule provider to use to validate entities added/updated to the service. By
        ///     default service will use data annotations to build business rules to validate the object.
        /// </param>
        /// <param name="handler">A handler to use to process entities being added/removed or activated through the provider.</param>
        public ProviderLinq(
            IUnitOfWorkLinq<TEntity, TKey> db,
            IEntityIdProvider<TEntity, TKey> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<TEntity> specProvider = null,
            EntityHandler<TEntity, TKey> handler = null)
        {
            Guard.ArgumentNotNull(db, nameof(db));
            Guard.ArgumentNotNull(messagePipe, nameof(messagePipe));

            Db = db;

            MessagePipe = messagePipe ?? new NoOpMessagePipe();
            _idProvider = idProvider;

            // wrap processors
            // ----------------------------------------------------
            _onBeforeDelete = new Stack<Action<TKey>>();
            _onBeforeInsert = new Stack<Action<TEntity>>();
            _onInitialize = new Stack<Action<TEntity>>();

            if (handler != null)
            {
                if (handler.RemoveHandler != null)
                    OnRemoving(handler.RemoveHandler.Handle);

                if (handler.AddHandler != null)
                    OnAdding(handler.AddHandler.Handle);

                if (handler.ActivateHandler != null)
                    OnInitializing(handler.ActivateHandler.Handle);
            }
            // ---------------------------------------------------

            //assign specs
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (specProvider != null)
                _specs = specProvider.GetSpecifications().ToList();
            else
                _specs = _defaultSpecs;
        }

        /// <summary>
        ///     Gets the id provider for the entity type.
        /// </summary>
        public IEntityIdProvider<TEntity, TKey> GetIdProvider()
        {
            return _idProvider;
        }

        // gets the user in the current thread/app context
        public virtual string GetCurrentUser()
        {
            return Thread.CurrentPrincipal?.Identity?.Name ?? Environment.UserName;
        }

        /// <summary>
        ///     Processes an item while it is removing.
        /// </summary>
        public void OnRemoving(Action<TKey> deleteHandler)
        {
            Guard.ArgumentNotNull(deleteHandler, nameof(deleteHandler));

            _onBeforeDelete.Push(deleteHandler);
        }

        /// <summary>
        ///     Processes the contrustruction/initialization of a given entity.
        /// </summary>
        public void OnInitializing(Action<TEntity> initializeHandler)
        {
            Guard.ArgumentNotNull(initializeHandler, nameof(initializeHandler));

            _onInitialize.Push(initializeHandler);
        }

        /// <summary>
        ///     Processes the addition of a given entity to the service's context.
        /// </summary>
        public void OnAdding(Action<TEntity> addHandler)
        {
            Guard.ArgumentNotNull(addHandler, nameof(addHandler));

            _onBeforeInsert.Push(addHandler);
        }


        private class NoOpMessagePipe : IMessagePipe
        {
            public Task SendAsync<T>(T message) where T : IDomainEvent
            {
                return Task.Run(() => { Trace.WriteLine(Convert.ToString(message)); });
            }
        }

        /// <summary>
        ///     Gets an entity by its primary key.
        /// </summary>
        public TEntity GetById(TKey key)
        {
            using (Db.Open())
            {
                return GetById(key, Db);
            }
        }

        /// <summary>
        ///     Gets an entity by its primary key from a given unit of work.
        /// </summary>
        public TEntity GetById(TKey key, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            //convert to hashset to make unique
            var entity = db.EntitySet.Reader.Get(key);

            if (entity != null)
                Initialize(entity);

            return entity;
        }

        /// <summary>
        ///     Selects a set of entities by a given expression.
        /// </summary>
        public IEnumerable<TEntity> Where(Expression<Func<TEntity, bool>> whereClause)
        {
            using (Db.Open())
            {
                return Where(whereClause, Db);
            }
        }

        /// <summary>
        ///     Selects a set of entities by a given expression.
        /// </summary>
        public IEnumerable<TEntity> Where(Expression<Func<TEntity, bool>> whereClause,
            IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(whereClause, nameof(whereClause));

            IList<TEntity> entities;

            entities = db.EntitySet.LinqReader.Where(whereClause).ToList();

            foreach (var item in entities)
                Initialize(item);

            return entities;
        }

        /// <summary>
        ///     Gets all entities registered in the service.
        /// </summary>
        public virtual IEnumerable<TEntity> GetAll()
        {
            using (Db.Open())
            {
                return GetAll(Db);
            }
        }

        /// <summary>
        ///     Gets all instances of an entity against a given unit of work.
        /// </summary>
        public virtual IEnumerable<TEntity> GetAll(IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(db, nameof(db));

            //convert to hashset to make unique
            return OnMaterialized(Db.EntitySet.Reader.GetAll());
        }

        /// <summary>
        ///     Gets a set of entities by a set of keys against an open unit of work.
        /// </summary>
        public IEnumerable<TEntity> GetByIds(IEnumerable<TKey> keys)
        {
            Guard.ArgumentNotNull(keys, nameof(keys));

            using (Db.Open())
            {
                return GetByIds(keys, Db); //convert to hashset to make unique
            }
        }

        /// <summary>
        ///     Gets a set of entities by a set of keys against an open unit of work.
        /// </summary>
        public IEnumerable<TEntity> GetByIds(IEnumerable<TKey> keys, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(keys, nameof(keys));
            Guard.ArgumentNotNull(db, nameof(db));

            //convert to hashset to make unique
            return OnMaterialized(db.EntitySet.Reader.GetByIds(keys.ToHashSetSafe()));
        }

        /// <summary>
        ///     Called whenever an entity is loaded from the database.
        /// </summary>
        protected virtual IEnumerable<TEntity> OnMaterialized(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                yield return Initialize(entity);
        }


        // TODO: consider internalizing

        /// <summary>
        ///     Called to attach an entity into the domain of the service.
        /// </summary>
        /// <remarks>
        ///     Sub-classes should implement this logic to decorate the entity with any attributes/behaviours required.
        /// </remarks>
        public virtual TEntity Initialize(TEntity entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            _onInitialize.Each(m => m(entity));

            return entity;
        }

        //an - used to project id values from data store
        public class IdClass
        {
            public TKey Id { get; set; }
        }

        /// <summary>
        ///     Adds and entity to the service.
        /// </summary>
        public virtual void Add(TEntity entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            Add(new[] {entity});
        }

        protected virtual void OnAfterItemAdded(TEntity entity)
        {
        }

        protected virtual void OnBeforeAdd(TEntity entity)
        {
            _onBeforeInsert.Each(m => m(entity));
        }

        /// <summary>
        ///     Adds a number of entities to the system and commits the changes.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        public void Add(IEnumerable<TEntity> entities)
        {
            Guard.ArgumentNotNull(entities, nameof(entities));

            //avoid multiple enumerations
            var entitiesAsArray = entities.ToArray();

            using (Db.Open())
            {
                Add(entitiesAsArray, Db);

                Db.Commit();
            }

            // bind to current service
            entitiesAsArray.Each(m => { Initialize(m); });
        }

        /// <summary>
        ///     Add and updates a set of entities within a transaction.
        /// </summary>
        /// <param name="entitiesToAdd">The entities to add.</param>
        /// <param name="entitiesToUpdate">The entities to update.</param>
        public void AddUpdate(IEnumerable<TEntity> entitiesToAdd, IEnumerable<TEntity> entitiesToUpdate)
        {
            Guard.ArgumentNotNull(entitiesToAdd, nameof(entitiesToUpdate));
            Guard.ArgumentNotNull(entitiesToUpdate, nameof(entitiesToUpdate));

            //avoid multiple enumerations
            var entitiesAsArray = entitiesToAdd.ToArray();

            var entitiesToUpdateArray = entitiesToUpdate.ToArray();

            using (Db.Open())
            {
                Add(entitiesAsArray, Db);

                Update(entitiesToUpdateArray, Db);

                Db.Commit();
            }

            // bind to current service
            entitiesAsArray.Each(m => { Initialize(m); });
        }

        /// <summary>
        ///     Update a batch of entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public void Update(IEnumerable<TEntity> entities)
        {
            Guard.ArgumentNotNull(entities, nameof(entities));

            //avoid multiple enumerations
            var entitiesAsArray = entities.ToArray();

            using (Db.Open())
            {
                Update(entitiesAsArray, Db);

                Db.Commit();
            }
        }

        /// <summary>
        ///     Update a batch of entities.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        public virtual void Update(IEnumerable<TEntity> entities, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(entities, nameof(entities));
            Guard.ArgumentNotNull(db, nameof(db));

            //avoid multiple enumerations
            var entitiesAsArray = entities.ToArray();

            entitiesAsArray.Each(OnBeforeUpdate);

            DemandValid(entitiesAsArray);

            Db.EntitySet.Writer.Update(entitiesAsArray);

            // send domain events
            entitiesAsArray.Each(OnAfterItemUpdated);
        }

        protected virtual void OnAfterItemUpdated(TEntity entity)
        {
            // todo
            MessagePipe.SendAsync(new ItemUpdated<TEntity>(entity));
        }

        /// <summary>
        ///     Adds a number of entities to the system.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="db">The unit of work to operate against.</param>
        public virtual void Add(IEnumerable<TEntity> entities, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(entities, nameof(entities));
            Guard.ArgumentNotNull(db, nameof(db));

            //avoid multiple enumerations
            var entitiesAsArray = entities.ToArray();

            entitiesAsArray.Each(OnBeforeAdd);

            DemandValid(entitiesAsArray);

            // try assign domain id
            entitiesAsArray.Each(entity =>
            {
                if (entity.Id.Equals(default(TKey)))
                    _idProvider.Provide(entity); //assign next key
            });

            Db.EntitySet.Writer.Insert(entitiesAsArray);

            // send domain events
            entitiesAsArray.Each(entity =>
                {
                    MessagePipe.SendAsync(new ItemAdded<TEntity>(entity));

                    // notify item add
                    OnAfterItemAdded(entity);
                }
            );
        }

        /// <summary>
        ///     Demands that a set of entities are valid against the service's set of specifications.
        /// </summary>
        public void DemandValid(params TEntity[] entities)
        {
            //avoid multiple enumerations
            Guard.ArgumentNotNull(entities, nameof(entities));

            //validate business logic
            entities.Each(
                entity =>
                {
                    _specs.ToErrors(entity).ThrowIfExists(); //validate
                });
        }

        /// <summary>
        ///     Gets an list of errors associated with the state of a given entity.
        /// </summary>
        public IEnumerable<Error> Validate(TEntity entity)
        {
            //avoid multiple enumerations
            Guard.ArgumentNotNull(entity, nameof(entity));

            return _specs.ToErrors(entity);
        }

        /// <summary>
        ///     Demands that a set of entities are valid against the service's set of specifications.
        /// </summary>
        public void DemandValid(TEntity entity)
        {
            DemandValid(new[] {entity});
        }

        /// <summary>
        ///     Updates a given entity and commits the changes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public void Update(TEntity entity)
        {
            using (Db.Open()) // connection to read will always be lazy loaded
            {
                Update(entity, Db);

                Db.Commit();
            }

            OnAfterItemUpdated(entity);
        }

        /// <summary>
        ///     Updates a given entity against a given unit of work.
        /// </summary>
        /// <remarks>
        ///     Caller must control commits to the given unit of work.
        /// </remarks>
        /// <param name="entity">The entity to update.</param>
        /// <param name="db">The unit of work instance to work against.</param>
        public virtual void Update(TEntity entity, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            Guard.ArgumentNotNull(db, nameof(db));

            //validate business logic
            DemandValid(entity);

            OnBeforeUpdate(entity);

            db.EntitySet.Writer.Update(entity);

            MessagePipe
                .SendAsync(new ItemUpdated<TEntity>(entity));
        }

        protected virtual void OnBeforeUpdate(TEntity entity)
        {
        }

        /// <summary>
        ///     Deletes an entity by its key and commits the changes.
        /// </summary>
        /// <param name="key">The id of the entity to delete.</param>
        public void RemoveById(TKey key)
        {
            using (Db.Open())
            {
                RemoveById(key, Db);

                Db.Commit();
            }
        }

        /// <summary>
        ///     Removes a set of entities matching a given set of criteria.
        /// </summary>
        /// <param name="criteria">The criteria to use to delete.</param>
        public void Remove(Expression<Func<TEntity, bool>> criteria)
        {
            using (Db.Open())
            {
                Remove(criteria, Db);

                Db.Commit();
            }
        }

        /// <summary>
        ///     Removes a set of entities matching a given set of criteria.
        /// </summary>
        /// <param name="criteria">The criteria to use to delete.</param>
        /// <param name="db">The unit of work to remove the entities matching the expression from.</param>
        public void Remove(Expression<Func<TEntity, bool>> criteria, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(db, nameof(db));
            Guard.ArgumentNotNull(criteria, nameof(criteria));

            // todo: optimize with projections
            var ids = db.EntitySet.LinqReader.Where(criteria).Select(m => m.Id).ToCollection();

            ids.Each(OnBeforeDelete);

            db.EntitySet.BulkDeleter.Delete(criteria);

            // TODO: send message via batch
            ids.Each(m => OnBeforeDeleteCommit(db, m));
        }

        /// <summary>
        ///     Deletes an entity by its key.
        /// </summary>
        /// <param name="key">The id of the entity to delete.</param>
        /// <param name="db">The unit of work instance to work against.</param>
        public virtual void RemoveById(TKey key, IUnitOfWorkLinq<TEntity, TKey> db)
        {
            Guard.ArgumentNotNull(db, nameof(db));

            // assume db session is open
            OnBeforeDelete(key);

            db.EntitySet.Writer.DeleteById(key);

            OnBeforeDeleteCommit(db, key);

            MessagePipe.SendAsync(new ItemDeleted<TEntity, TKey>(key));
        }

        protected virtual void OnBeforeDeleteCommit(IUnitOfWorkLinq<TEntity, TKey> db, TKey key)
        {
        }

        /// <summary>
        ///     Will be called with an open database session.
        /// </summary>
        protected virtual void OnBeforeDelete(TKey key)
        {
            _onBeforeDelete.Each(m => m(key));
        }
    }
}