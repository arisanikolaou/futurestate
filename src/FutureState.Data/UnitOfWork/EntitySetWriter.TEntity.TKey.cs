using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Data
{
    /// <summary>
    ///     Adapts a repository to write entities via a repository
    ///     throgh a consistent data store session.
    /// </summary>
    public class EntitySetWriter<TEntity, TKey> : IWriter<TEntity, TKey>
    {
        private readonly Func<ISession, IRepository<TEntity, TKey>> _repositoryFunc;
        private readonly UnitOfWork _uow;

        internal EntitySetWriter(UnitOfWork work, Func<ISession, IRepository<TEntity, TKey>> repositoryFunc)
        {
            Guard.ArgumentNotNull(repositoryFunc, "repositoryFunc");

            // internal class assume valid inputs
            _uow = work;
            _repositoryFunc = repositoryFunc;
        }

        /// <summary>
        ///     Gets the writer to the underlying data store.
        /// </summary>
        private IWriter<TEntity, TKey> Writer => _repositoryFunc.Invoke(_uow.Session);


        public void Delete(TEntity entity)
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.Delete(entity));
            _uow._deleted.Add(entity);
        }


        public void DeleteAll()
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.DeleteAll());
            _uow._deleted.Add(_repositoryFunc.Invoke(_uow.Session).GetAll());
        }

        public void DeleteById(TKey key)
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.DeleteById(key));
            _uow._deleted.Add(key); // don't add entity
        }

        public void Insert(TEntity entity)
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.Insert(entity));
            _uow._inserted.Add(entity);
        }

        public void Insert(IEnumerable<TEntity> entities)
        {
            ValidateAction();

            // avoid deferred execution
            var local = entities as ICollection<TEntity> ?? entities.ToArray();

            _uow._executionQueue.Enqueue(() => Writer.Insert(local));

            foreach (var entity in local)
                _uow._inserted.Add(entity);
        }

        public void Update(TEntity entity)
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.Update(entity));
            _uow._modified.Add(entity);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            Guard.ArgumentNotNull(entities, "entities");

            ValidateAction();

            //to avoid double possible enumeration
            var entityList = entities.ToList();

            _uow._executionQueue.Enqueue(() => Writer.Update(entityList));
            entityList.Each(entity => _uow._modified.Add(entity));
        }


        private void ValidateAction()
        {
            if (_uow.IsDisposed)
                throw new InvalidOperationException(
                    "The underlying session has been disposed and can no longer be written or read from.");
        }
    }
}