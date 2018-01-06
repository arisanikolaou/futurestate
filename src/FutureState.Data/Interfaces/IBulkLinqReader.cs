using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FutureState.Data
{
    /// <summary>
    ///     Selects a set of guid keyed entities by a set of parameters.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IBulkLinqReader<TEntity> : IBulkLinqReader<TEntity, Guid>
        //do not delete 'redundant' key
    {
    }

    /// <summary>
    ///     Can select distinct entities by a given set of query criteria that has a maximum
    ///     value for a specified column/property name.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TK">The entity key.</typeparam>
    public interface IBulkLinqReader<TEntity, in TK> : ILinqReader<TEntity, TK>
        //do not delete 'redundant' key
    {
        /// <summary>
        ///     Queries a sub-set of a given table for entities that match a given set of criteria and will select
        ///     the maximum top record given this criteria based on a given date or numeric field of the entity such as a date time
        ///     field.
        /// </summary>
        /// <remarks>
        ///     Emits a sub-query to query the underlying db server.
        /// </remarks>
        /// <example>
        ///     MarketQuote[] entities = ormLite.WhereMax(
        ///     new MarketQuoteArg[]
        ///     {
        ///     new MarketQuoteArg(){ IdIntern = @"123"},
        ///     new MarketQuoteArg(){ IdIntern = @"124"}
        ///     },
        ///     //filter out expression
        ///     (marketQuote, quoteArg) => marketQuote.IdIntern == quoteArg.IdIntern,
        ///     m => m.Date,
        ///     m => m.IdIntern != "121" && m.Date less than (yesterday),
        ///     m => m.IdIntern).ToArray();
        /// </example>
        /// <typeparam name="TQueryArg">The query argument type. This should not be the same as the entity type.</typeparam>
        /// <param name="queryArgs">The set of query arguments to match and filter to.</param>
        /// <param name="matchingExpression">The grouping expression to match and compare the entity values to the query arg.</param>
        /// <param name="maxEntityColumnKeyExpression">The single column to select the entity max value for.</param>
        /// <param name="whereExpression">Any filtering expression from the unioned result.</param>
        /// <param name="orderByExpression">How to order the results in descending order.</param>
        /// <returns>
        ///     An array of zero or more matching entities.
        /// </returns>
        IEnumerable<TEntity> GetTopByKeys<TQueryArg>(
            IEnumerable<TQueryArg> queryArgs,
            Expression<Func<TEntity, TQueryArg, bool>> matchingExpression,
            Expression<Func<TEntity, object>> maxEntityColumnKeyExpression,
            Expression<Func<TEntity, bool>> whereExpression,
            Expression<Func<TEntity, object>> orderByExpression);
    }
}