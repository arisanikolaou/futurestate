using Dapper.FastCrud.EntityDescriptors;
using Dapper.FastCrud.Mappings;
using System;

namespace Dapper.FastCrud.SqlBuilders
{
    internal class SqLiteBuilder : GenericStatementSqlBuilder
    {
        public SqLiteBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.SqLite)
        {
        }

        /// <summary>
        ///     Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            var sql = ResolveWithCultureInvariantFormatter(
                $"INSERT INTO {GetTableName()} ({ConstructColumnEnumerationForInsert()}) VALUES ({ConstructParamEnumerationForInsert()}); ");

            if (RefreshOnInsertProperties.Length > 0)
                if (InsertKeyDatabaseGeneratedProperties.Length == 1 && RefreshOnInsertProperties.Length == 1)
                    sql +=
                        ResolveWithCultureInvariantFormatter(
                            $"SELECT last_insert_rowid() as {GetDelimitedIdentifier(InsertKeyDatabaseGeneratedProperties[0].PropertyName)};");
                else
                    sql +=
                        ResolveWithCultureInvariantFormatter(
                            $"SELECT {ConstructRefreshOnInsertColumnSelection()} FROM {GetTableName()} WHERE {GetDelimitedIdentifier("_ROWID_")}=last_insert_rowid();");

            return sql;
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

            if (limitRowsCount.HasValue || skipRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" LIMIT {limitRowsCount ?? -1}");
            if (skipRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" OFFSET {skipRowsCount}");

            return sql;
        }
    }
}