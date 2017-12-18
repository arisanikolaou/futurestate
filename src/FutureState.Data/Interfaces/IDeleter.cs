using System;
using System.Linq.Expressions;

namespace FutureState.Data
{
    public interface IDeleter<in T, in TK>
    {
        void Delete(T item);

        void DeleteById(TK key);
    }

    public interface IDeleter<in T> : IDeleter<T, Guid>
    {
    }

    /// <summary>
    ///     Deletes a set of records by a given predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TK">The entity's key type.</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IBulkDeleter<T, in TK> //do not delete 'redundant' key
    {
        void Delete(Expression<Func<T, bool>> predicate);
    }

    /// <summary>
    ///     Deletes a set of guid keyed entities by a given predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IBulkDeleter<T> : IBulkDeleter<T, Guid> //do not delete 'redundant' key
    {
    }
}