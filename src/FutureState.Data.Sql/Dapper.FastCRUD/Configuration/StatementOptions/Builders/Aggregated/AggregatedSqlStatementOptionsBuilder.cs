using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
using Dapper.FastCrud.Mappings;
using Dapper.FastCrud.Validations;
using System;
using System.Data;

namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    /// <summary>
    ///     Common options builder for all the non-JOIN type queries.
    /// </summary>
    internal abstract class AggregatedSqlStatementOptionsBuilder<TEntity, TStatementOptionsBuilder> :
        AggregatedSqlStatementOptions<TEntity>
    {
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        ///     Limits the results set by the top number of records returned.
        /// </summary>
        public TStatementOptionsBuilder Top(long? topRecords)
        {
            Requires.Argument(topRecords == null || topRecords > 0, nameof(topRecords),
                "The top record count must be a positive value");

            LimitResults = topRecords;
            return Builder;
        }

        /// <summary>
        ///     Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString orderByClause)
        {
            OrderClause = orderByClause;
            return Builder;
        }

        /// <summary>
        ///     Skips the initial set of results.
        /// </summary>
        public TStatementOptionsBuilder Skip(long? skipRecordsCount)
        {
            Requires.Argument(skipRecordsCount == null || skipRecordsCount >= 0, nameof(skipRecordsCount),
                "The number of records to skip must be a positive value");

            SkipResults = skipRecordsCount;
            return Builder;
        }

        /// <summary>
        ///     Causes the result set to be streamed.
        /// </summary>
        public TStatementOptionsBuilder StreamResults()
        {
            ForceStreamResults = true;
            return Builder;
        }

        /// <summary>
        ///     Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsBuilder Where(FormattableString whereClause)
        {
            WhereClause = whereClause;
            return Builder;
        }

        /// <summary>
        ///     Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsBuilder WithParameters(object parameters)
        {
            Parameters = parameters;
            return Builder;
        }

        /// <summary>
        ///     Enforces a maximum time span on the current command.
        /// </summary>
        public TStatementOptionsBuilder WithTimeout(TimeSpan? commandTimeout)
        {
            CommandTimeout = commandTimeout;
            return Builder;
        }

        /// <summary>
        ///     Attaches the current command to an existing transaction.
        /// </summary>
        public TStatementOptionsBuilder AttachToTransaction(IDbTransaction transaction)
        {
            Transaction = transaction;
            return Builder;
        }

        /// <summary>
        ///     Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity> entityMapping)
        {
            EntityMappingOverride = entityMapping;
            return Builder;
        }

        /// <summary>
        ///     Includes a referred entity into the query. The relationship and the associated mappings must be set up prior to
        ///     calling this method.
        /// </summary>
        public TStatementOptionsBuilder Include<TReferredEntity>(
            Action<ISqlRelationOptionsBuilder<TReferredEntity>> relationshipOptions = null)
        {
            // set up the relationship options
            var options = new SqlRelationOptionsBuilder<TReferredEntity>();
            relationshipOptions?.Invoke(options);
            RelationshipOptions[typeof(TReferredEntity)] = options;

            // set up the factory chain
            var priorSqlStatementsFactoryChain = SqlStatementsFactoryChain;

            SqlStatementsFactoryChain = () =>
            {
                var currentSqlStatementsFactory = priorSqlStatementsFactoryChain();
                var nextSqlStatementsFactory =
                    currentSqlStatementsFactory.CombineWith(
                        OrmConfiguration.GetSqlStatements<TReferredEntity>(options.EntityMappingOverride));
                return nextSqlStatementsFactory;
            };

            return Builder;
        }
    }
}