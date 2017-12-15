#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// A reader for a set of for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity to read.</typeparam>
    /// <typeparam name="TKey">The entity key.</typeparam>
    public interface ILinqReader<TEntity, in TKey> : IPagedReader<TEntity, TKey>
    {
        /// <summary>
        /// Gets whether any items matching a given expression exist.
        /// </summary>
        bool Any(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the number of entities matching a given predicate.
        /// </summary>
        long Count(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets the first entity matching the expression.
        /// </summary>
        /// <param name="predicate">The predicate to search for.</param>
        /// <returns></returns>
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Finds a set of entities based on a given set of descriptors and an expression.
        /// </summary>
        /// <typeparam name="TQueryArg">
        /// A descriptor type which must be decorated with an Alias attribute: [Alias("#descriptors")]
        /// Internally, this type is defined as a table, thus it must define a primary key,
        /// the Id property with the AutoIncrement attriubte can be used in this occasion.
        /// Preferably, it should define a composite unique key for faster performance.
        /// </typeparam>
        /// <param name="queryArgs">A set of descriptors.</param>
        /// <param name="matchExpression">An expression on how to use the supplied query arguments.</param>
        /// <returns>A list of entities.</returns>
        IEnumerable<TEntity> GetByKeys<TQueryArg>(IEnumerable<TQueryArg> queryArgs,
            Expression<Func<TEntity, TQueryArg, bool>> matchExpression);

        /// <summary>
        /// Select subset of columns using the specified row selection criteria.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of object to return from the query.
        /// The projection class must have a subset of desired fields defined.
        /// </typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IEnumerable<TProjection> Select<TProjection>(Expression<Func<TEntity, bool>> predicate)
            where TProjection : new();

        /// <summary>
        /// Select subset of columns.
        /// </summary>
        /// <typeparam name="TProjection">
        /// The type of object to return from the query.
        /// The projection class must have a subset of desired fields defined.
        /// </typeparam>
        /// <returns></returns>
        IEnumerable<TProjection> Select<TProjection>()
            where TProjection : new();

        /// <summary>
        /// Finds a single matching entity given a predicate.
        /// </summary>
        /// <param name="predicate">The query to search for.</param>
        /// <returns>null if not found</returns>
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Finds a set of entities based on a given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>A list of zero or more entities.</returns>
        IEnumerable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    }

    public interface ILinqReader<TEntity> : ILinqReader<TEntity, Guid>, IPagedReader<TEntity>
    {
    }
}