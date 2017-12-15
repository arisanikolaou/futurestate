using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
using Dapper.FastCrud.Mappings;
using Dapper.FastCrud.SqlBuilders;

namespace Dapper.FastCrud.SqlStatements
{
    internal class GenericSqlStatements<TEntity> : ISqlStatements<TEntity>
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public GenericSqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            SqlBuilder = sqlBuilder;
        }

        /// <summary>
        ///     Gets the publicly accessible SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder { get; }

        /// <summary>
        ///     Combines the current instance with a joined entity.
        /// </summary>
        public ISqlStatements<TEntity> CombineWith<TJoinedEntity>(
            ISqlStatements<TJoinedEntity> joinedEntitySqlStatements)
        {
            return new TwoEntitiesRelationshipSqlStatements<TEntity, TJoinedEntity>(this,
                joinedEntitySqlStatements.SqlBuilder);
        }

        /// <summary>
        ///     Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity SelectById(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.Query<TEntity>(
                SqlBuilder.ConstructFullSingleSelectStatement(),
                keyEntity,
                statementOptions.Transaction,
                commandTimeout: (int?) statementOptions.CommandTimeout?.TotalSeconds).SingleOrDefault();
        }

        /// <summary>
        ///     Performs an async SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TEntity> SelectByIdAsync(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return (await connection.QueryAsync<TEntity>(
                SqlBuilder.ConstructFullSingleSelectStatement(),
                keyEntity,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds)).SingleOrDefault();
        }

        /// <summary>
        ///     Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IDbConnection connection, TEntity entity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            if (SqlBuilder.RefreshOnInsertProperties.Length > 0)
            {
                var insertStatement = SqlBuilder.ConstructFullInsertStatement();

                var insertedEntity =
                    connection.Query<TEntity>(
                        insertStatement,
                        entity,
                        statementOptions.Transaction,
                        commandTimeout: (int?) statementOptions.CommandTimeout?.TotalSeconds)
                        .FirstOrDefault();

                // copy all the database generated props back onto our entity
                CopyEntity(insertedEntity, entity, SqlBuilder.RefreshOnInsertProperties);
            }
            else
            {
                connection.Execute(
                    SqlBuilder.ConstructFullInsertStatement(),
                    entity,
                    statementOptions.Transaction,
                    (int?) statementOptions.CommandTimeout?.TotalSeconds);
            }
        }

        /// <summary>
        ///     Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InsertAsync(IDbConnection connection, TEntity entity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            if (SqlBuilder.RefreshOnInsertProperties.Length > 0)
            {
                var insertedEntity =
                (await
                    connection.QueryAsync<TEntity>(
                        SqlBuilder.ConstructFullInsertStatement(),
                        entity,
                        statementOptions.Transaction,
                        (int?) statementOptions.CommandTimeout?.TotalSeconds)).FirstOrDefault();
                // copy all the database generated props back onto our entity
                CopyEntity(insertedEntity, entity, SqlBuilder.RefreshOnInsertProperties);
            }
            else
            {
                connection.Execute(
                    SqlBuilder.ConstructFullInsertStatement(),
                    entity,
                    statementOptions.Transaction,
                    (int?) statementOptions.CommandTimeout?.TotalSeconds);
            }
        }

        /// <summary>
        ///     Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateById(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            if (SqlBuilder.RefreshOnUpdateProperties.Length > 0)
            {
                var updatedEntity = connection.Query<TEntity>(
                    SqlBuilder.ConstructFullSingleUpdateStatement(),
                    keyEntity,
                    statementOptions.Transaction,
                    commandTimeout: (int?) statementOptions.CommandTimeout?.TotalSeconds).FirstOrDefault();

                if (updatedEntity != null)
                    CopyEntity(updatedEntity, keyEntity, SqlBuilder.RefreshOnUpdateProperties);

                return updatedEntity != null;
            }

            return connection.Execute(
                       SqlBuilder.ConstructFullSingleUpdateStatement(),
                       keyEntity,
                       statementOptions.Transaction,
                       (int?) statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        ///     Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> UpdateByIdAsync(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            if (SqlBuilder.RefreshOnUpdateProperties.Length > 0)
            {
                var updatedEntity = (await connection.QueryAsync<TEntity>(
                    SqlBuilder.ConstructFullSingleUpdateStatement(),
                    keyEntity,
                    statementOptions.Transaction,
                    (int?) statementOptions.CommandTimeout?.TotalSeconds)).FirstOrDefault();

                if (updatedEntity != null)
                    CopyEntity(updatedEntity, keyEntity, SqlBuilder.RefreshOnUpdateProperties);

                return updatedEntity != null;
            }

            return await connection.ExecuteAsync(
                       SqlBuilder.ConstructFullSingleUpdateStatement(),
                       keyEntity,
                       statementOptions.Transaction,
                       (int?) statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        ///     Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkUpdate(IDbConnection connection, TEntity entity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.Execute(
                SqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause),
                entity,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.ExecuteAsync(
                SqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause),
                entity,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeleteById(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.Execute(
                       SqlBuilder.ConstructFullSingleDeleteStatement(),
                       keyEntity,
                       statementOptions.Transaction,
                       (int?) statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        ///     Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity,
            AggregatedSqlStatementOptions<TEntity> statementoptions)
        {
            return await connection.ExecuteAsync(
                       SqlBuilder.ConstructFullSingleDeleteStatement(),
                       keyEntity,
                       statementoptions.Transaction,
                       (int?) statementoptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        ///     Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.Execute(
                SqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkDeleteAsync(IDbConnection connection,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.ExecuteAsync(
                SqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.ExecuteScalar<int>(
                SqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.ExecuteScalarAsync<int>(
                SqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a common SELECT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {

            return connection.Query<TEntity>(
                SqlBuilder.ConstructFullBatchSelectStatement(
                    statementOptions.WhereClause,
                    statementOptions.OrderClause,
                    statementOptions.SkipResults,
                    statementOptions.LimitResults),
                statementOptions.Parameters,
                buffered: !statementOptions.ForceStreamResults,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        ///     Performs a common SELECT
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection,
            AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return connection.QueryAsync<TEntity>(
                SqlBuilder.ConstructFullBatchSelectStatement(
                    statementOptions.WhereClause,
                    statementOptions.OrderClause,
                    statementOptions.SkipResults,
                    statementOptions.LimitResults),
                statementOptions.Parameters,
                statementOptions.Transaction,
                (int?) statementOptions.CommandTimeout?.TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CopyEntity(TEntity source, TEntity destination, PropertyMapping[] properties)
        {
            foreach (var propMapping in properties)
            {
                var propDescriptor = propMapping.Descriptor;
                var updatedKeyValue = propDescriptor.GetValue(source);
                propDescriptor.SetValue(destination, updatedKeyValue);
            }
        }
    }
}