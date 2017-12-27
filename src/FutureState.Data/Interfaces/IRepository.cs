using System;
using System.Collections.Generic;

namespace FutureState.Data
{
    /// <summary>
    ///     Represents a repository of entities to read and write to.
    /// </summary>
    /// <typeparam name="TEntity">The entity to save, update and/or delete.</typeparam>
    /// <typeparam name="TKey">The entity's primary key.</typeparam>
    public interface IRepository<TEntity, in TKey> :
        IWriter<TEntity, TKey>,
        IReader<TEntity, TKey>
    {
    }

    /// <summary>
    ///     Represents a repository of entities to read and write to.
    /// </summary>
    public interface IRepository<TEntity> : IRepository<TEntity, Guid>,
        IDeleter<TEntity>,
        ILinqReader<TEntity>
    {
    }

    /// <summary>
    ///     A specialized type of repository used to read/write keyed entities in bulk.
    /// </summary>
    public interface IBulkRepository<TEntity, in TKey> //don't inherit from irepository
    {
        void DeleteByIds(IEnumerable<TKey> ids);

        IEnumerable<TEntity> GetByIds(IEnumerable<TKey> ids);

        void SaveOrUpdate(IEnumerable<TEntity> entities);
    }

    /// <summary>
    ///     A specialized type of repository used to read/write keyed entities in bulk.
    /// </summary>
    public interface IBulkRepository<TEntity> : IBulkRepository<TEntity, Guid>
    {
    }
}