using Dapper.FastCrud.EntityDescriptors;
using Dapper.FastCrud.Mappings;
using System;

namespace Dapper.FastCrud.SqlBuilders
{
    internal class PostgreSqlBuilder : GenericStatementSqlBuilder
    {
        public PostgreSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.PostgreSql)
        {
        }

        /// <summary>
        ///     Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            var outputQuery = RefreshOnInsertProperties.Length > 0
                ? ResolveWithCultureInvariantFormatter($"RETURNING {ConstructRefreshOnInsertColumnSelection()}")
                : string.Empty;

            return
                ResolveWithCultureInvariantFormatter(
                    $"INSERT INTO {GetTableName()} ({ConstructColumnEnumerationForInsert()}) VALUES ({ConstructParamEnumerationForInsert()}) {outputQuery}");
        }

        protected override string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            bool forceTableColumnResolution = false)
        {
            var sql = ResolveWithCultureInvariantFormatter($"SELECT {selectClause} FROM {fromClause}");

            if (whereClause != null)
                sql += " WHERE " + ResolveWithSqlFormatter(whereClause, forceTableColumnResolution);
            if (orderClause != null)
                sql += " ORDER BY " + ResolveWithSqlFormatter(orderClause, forceTableColumnResolution);
            if (limitRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" LIMIT {limitRowsCount}");
            if (skipRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" OFFSET {skipRowsCount}");

            return sql;
        }
    }
}