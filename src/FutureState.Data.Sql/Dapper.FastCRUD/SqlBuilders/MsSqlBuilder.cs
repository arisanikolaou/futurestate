using System;
using System.Linq;
using Dapper.FastCrud.EntityDescriptors;
using Dapper.FastCrud.Mappings;

namespace Dapper.FastCrud.SqlBuilders
{
    internal class MsSqlBuilder : GenericStatementSqlBuilder
    {
        public MsSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.MsSql)
        {
        }

        /// <summary>
        ///     Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            if (RefreshOnInsertProperties.Length == 0)
                return
                    ResolveWithCultureInvariantFormatter(
                        $"INSERT INTO {GetTableName()} ({ConstructColumnEnumerationForInsert()}) VALUES ({ConstructParamEnumerationForInsert()})");

            // one database generated field to be inserted, and that alone is a the primary key
            if (InsertKeyDatabaseGeneratedProperties.Length == 1 && RefreshOnInsertProperties.Length == 1)
            {
                var keyProperty = InsertKeyDatabaseGeneratedProperties[0];
                var keyPropertyType = keyProperty.Descriptor.PropertyType;

                if (keyPropertyType == typeof(int) || keyPropertyType == typeof(long))
                    return
                        ResolveWithCultureInvariantFormatter(
                            $@"INSERT 
                                    INTO {GetTableName()} ({ConstructColumnEnumerationForInsert()}) 
                                    VALUES ({ConstructParamEnumerationForInsert()});
                           SELECT SCOPE_IDENTITY() AS {GetDelimitedIdentifier(keyProperty.PropertyName)}");
            }

            var dbInsertedOutputColumns = string.Join(",",
                RefreshOnInsertProperties.Select(propInfo => $"inserted.{GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = ConstructRefreshOnInsertColumnSelection();

            // the union will make the constraints be ignored
            return ResolveWithCultureInvariantFormatter($@"
                SELECT *
                    INTO #temp 
                    FROM (SELECT {dbGeneratedColumns} FROM {GetTableName()} WHERE 1=0 
                        UNION SELECT {dbGeneratedColumns} FROM {GetTableName()} WHERE 1=0) as u;
            
                INSERT INTO {GetTableName()} ({ConstructColumnEnumerationForInsert()}) 
                    OUTPUT {dbInsertedOutputColumns} INTO #temp 
                    VALUES ({ConstructParamEnumerationForInsert()});

                SELECT * FROM #temp");
        }

        /// <summary>
        ///     Constructs an update statement for a single entity.
        /// </summary>
        protected override string ConstructFullSingleUpdateStatementInternal()
        {
            if (RefreshOnUpdateProperties.Length == 0)
                return base.ConstructFullSingleUpdateStatementInternal();

            var dbUpdatedOutputColumns = string.Join(",",
                RefreshOnUpdateProperties.Select(propInfo => $"inserted.{GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = string.Join(",",
                RefreshOnUpdateProperties.Select(propInfo => $"{GetColumnName(propInfo, null, true)}"));

            // the union will make the constraints be ignored
            return ResolveWithCultureInvariantFormatter($@"
                SELECT *
                    INTO #temp 
                    FROM (SELECT {dbGeneratedColumns} FROM {GetTableName()} WHERE 1=0 
                        UNION SELECT {dbGeneratedColumns} FROM {GetTableName()} WHERE 1=0) as u;

                UPDATE {GetTableName()} 
                    SET {ConstructUpdateClause()}
                    OUTPUT {dbUpdatedOutputColumns} INTO #temp
                    WHERE {ConstructKeysWhereClause()}

                SELECT * FROM #temp");
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
            if (skipRowsCount.HasValue || limitRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" OFFSET {skipRowsCount ?? 0} ROWS");
            if (limitRowsCount.HasValue)
                sql += ResolveWithCultureInvariantFormatter($" FETCH NEXT {limitRowsCount} ROWS ONLY");

            return sql;
        }
    }
}