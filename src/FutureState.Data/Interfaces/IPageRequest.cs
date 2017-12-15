#region

using System;
using System.Linq.Expressions;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// A request to page the results of a given query against a set of entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to page.</typeparam>
    public interface IPageRequest<TEntity>
    {
        /// <summary>
        /// Sorts the results in ascending order.
        /// </summary>
        IPageRequest<TEntity> Asc(Expression<Func<TEntity, object>> sortExpression);

        /// <summary>
        /// Sorts the results in descending order.
        /// </summary>
        IPageRequest<TEntity> Desc(Expression<Func<TEntity, object>> sortExpression);

        /// <summary>
        /// Filter the source prior to page selection.
        /// </summary>
        /// <param name="filterExpression">page inclusion criteria</param>
        /// <returns></returns>
        IPageRequest<TEntity> SetFilter(Expression<Func<TEntity, bool>> filterExpression);

        /// <summary>
        /// Sets the page number to a value that must be greater than 1.
        /// </summary>
        /// <param name="page">Must be a value greater than or equal to 1.</param>
        IPageRequest<TEntity> SetPageNumber(int page);

        /// <summary>
        /// </summary>
        /// <param name="size">Must be a value greater than or equal to 1.</param>
        IPageRequest<TEntity> SetPageSize(int size);
    }
}