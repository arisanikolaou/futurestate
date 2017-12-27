using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.FastCrud;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel.Mapping;
using Humanizer;

// ReSharper disable StaticMemberInGenericType

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     Sql repository implementation.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
#pragma warning disable RECS0108 // Warns about static fields in generic types
        protected static readonly string _tableName;
#pragma warning restore RECS0108 // Warns about static fields in generic types
        protected static readonly string _idColumnName;
        private static readonly string _dtTypeName = "GUID_ITEMS";
        private static readonly DataTable _modelTvpType;
        protected readonly SqlConnection _connection;
        private readonly Session _dbSession;

        static Repository()
        {
            var entityType = typeof(TEntity);

            DommelEntityMap<TEntity> map = null;

            // TODO: remove static dependency
            if (FluentMapper.EntityMaps.ContainsKey(entityType))
                map = FluentMapper.EntityMaps[entityType] as DommelEntityMap<TEntity>;

            if (map != null)
            {
                _tableName = map.TableName;
                _idColumnName = map.PropertyMaps
                    .Where(m => m.PropertyInfo.Name == "Id")
                    .Select(m => m.ColumnName).FirstOrDefault();
            }
            else
            {
                _tableName = typeof(TEntity).Name.Pluralize();
                _idColumnName = "Id";
            }

            // determine bulk delete
            if (typeof(TKey) == typeof(Guid))
                _dtTypeName = "GUID_ITEMS";
            else if (typeof(TKey) == typeof(string))
                _dtTypeName = "TEXT_ITEMS";
            else if (typeof(TKey) == typeof(DateTime))
                _dtTypeName = "DATE_ITEMS";
            else if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(long))
                _dtTypeName = "INT_ITEMS";
            else if (typeof(TKey) == typeof(bool))
                _dtTypeName = "BIT_ITEMS";

            var dt = new DataTable(_dtTypeName);
            dt.Columns.Add("Item", typeof(TKey));
            dt.PrimaryKey = new[] {dt.Columns[0]};

            _modelTvpType = dt;
        }

        public Repository(ISession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            _dbSession = session as Session;
            if (_dbSession != null)
                _connection = _dbSession.GetConnection();
            else
                throw new InvalidOperationException("ISession is not convertable to Session.");
        }

        /// <summary>
        ///     Inserts a new item.
        /// </summary>
        /// <param name="item"></param>
        public void Insert(TEntity item)
        {
            _connection.Insert(item,
                builder => builder.AttachToTransaction(GetCurrentTran()));
        }

        /// <summary>
        ///     Updates an entity
        /// </summary>
        /// <param name="item">The item to update.</param>
        public void Update(TEntity item)
        {
            _connection.Update(item,
                builder => builder.AttachToTransaction(GetCurrentTran()));
        }

        /// <summary>
        ///     Updates a set of entities.
        /// </summary>
        /// <param name="entities"></param>
        public void Update(IEnumerable<TEntity> entities)
        {
            // TODO: bulk update
            foreach (var entity in entities)
                _connection.Update(entity,
                    builder => builder.AttachToTransaction(GetCurrentTran()));
        }

        /// <summary>
        ///     Deletes an entity by its key.
        /// </summary>
        /// <param name="entity">The item to delete.</param>
        public void Delete(TEntity entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            DeleteById(entity.Id);
        }

        /// <summary>
        ///     Deletes an entity by its key.
        /// </summary>
        public void DeleteById(TKey key)
        {
            _connection.Execute($"Delete From [{_tableName}] Where [{_idColumnName}] = @id", new {id = key},
                GetCurrentTran());
        }

        /// <summary>
        ///     Bulk inserts a set of items.
        /// </summary>
        /// <param name="items"></param>
        public void Insert(IEnumerable<TEntity> items)
        {
            // TODO: insert via tvp parameter
            foreach (var item in items)
                _connection.Insert(item,
                    builder => builder.AttachToTransaction(GetCurrentTran()));
        }

        /// <summary>
        ///     Deletes all entities.
        /// </summary>
        public void DeleteAll()
        {
            _connection.Execute($"Delete From [{_tableName}]", null, GetCurrentTran());
        }

        /// <summary>
        ///     Gets an entity by its key;
        /// </summary>
        public TEntity Get(TKey key)
        {
            return _connection.QueryFirstOrDefault<TEntity>(
                $"Select * From [{_tableName}] Where [{_idColumnName}] = @id", new
                {
                    id = key
                }, GetCurrentTran());
        }

        /// <summary>
        ///     Gets all entities associated with the current instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll()
        {
            return _connection.Query<TEntity>($"Select * From [{_tableName}]", null, GetCurrentTran());
        }

        /// <summary>
        ///     Gets whether any entities have been added to the current instance.
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return Count() > 0;
        }

        /// <summary>
        ///     Gets the number of entities saved in the current instance.
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return
                Convert.ToInt64(_connection.ExecuteScalar($"Select Count(*) From [{_tableName}]", null,
                    GetCurrentTran()));
        }

        public IEnumerable<TEntity> GetByIds(IEnumerable<TKey> ids)
        {
            var dt = _modelTvpType.Copy();

            // add ids
            ids.Each(m => dt.Rows.Add(m));

            var tvp = dt.AsTableValuedParameter(_dtTypeName);

            return
                _connection.Query<TEntity>(
                    $"Select  * From [{_tableName}] Where [{_idColumnName}] In (Select [Item] From @keys)",
                    new
                    {
                        keys = tvp
                    }, GetCurrentTran());
        }

        private IDbTransaction GetCurrentTran()
        {
            var tran = _dbSession.GetCurrentTran() as Transacton;

            return tran?.UnderlyingTransaction;
        }
    }
}