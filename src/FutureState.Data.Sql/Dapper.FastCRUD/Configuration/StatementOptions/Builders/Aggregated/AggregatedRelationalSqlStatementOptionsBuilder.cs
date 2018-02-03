using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
using Dapper.FastCrud.Mappings;
using System;

namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    /// <summary>
    ///     Common options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptionsBuilder<TReferredEntity, TStatementOptionsBuilder> :
        AggregatedRelationalSqlStatementOptions
    {
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        ///     Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity> entityMapping)
        {
            EntityMappingOverride = entityMapping;
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
        ///     Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString orderByClause)
        {
            OrderClause = orderByClause;
            return Builder;
        }

        /// <summary>
        ///     A left outer join is desired.
        /// </summary>
        public TStatementOptionsBuilder LeftOuterJoin()
        {
            JoinType = SqlJoinType.LeftOuterJoin;
            return Builder;
        }

        /// <summary>
        ///     An inner join is desired.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin()
        {
            JoinType = SqlJoinType.InnerJoin;
            return Builder;
        }
    }
}