using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Implementor;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using Dapper.Extensions.Linq.Predicates;
using FutureState;
using FutureState.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace Dapper.Extensions.Linq.Implementor
{
    public class DapperImplementor : IDapperImplementor
    {
        public DapperImplementor(ISqlGenerator sqlGenerator)
        {
            SqlGenerator = sqlGenerator;
        }

        public ISqlGenerator SqlGenerator { get; }

        public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetIdPredicate(classMap, id);
            var result =
                GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true, 1, false)
                    .SingleOrDefault();
            return result;
        }

        public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction,
            int? commandTimeout) where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var properties = classMap.LinqPropertyMaps.Where(p => p.KeyType != KeyType.NotAKey).ToList();

            foreach (var e in entities)
                foreach (var column in properties)
                    if (column.KeyType == KeyType.Guid)
                    {
                        var comb = SeqGuid.Create();
                        column.PropertyInfo.SetValue(e, comb, null);
                    }

            var sql = SqlGenerator.Insert(classMap);

            connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
        }

        public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties =
                classMap.LinqPropertyMaps.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned)
                    .ToList();
            var identityColumn = classMap.LinqPropertyMaps.SingleOrDefault(p => p.KeyType == KeyType.Identity);

            foreach (var column in nonIdentityKeyProperties)
                if (column.KeyType == KeyType.Guid)
                {
                    var comb = SeqGuid.Create();
                    column.PropertyInfo.SetValue(entity, comb, null);
                }

            IDictionary<string, object> keyValues = new ExpandoObject();
            var sql = SqlGenerator.Insert(classMap);
            if (identityColumn != null)
            {
                IEnumerable<long> result;
                if (SqlGenerator.SupportsMultipleStatements())
                {
                    sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }
                else
                {
                    connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
                    sql = SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                }

                var identityValue = result.First();
                var identityInt = Convert.ToInt32(identityValue);
                keyValues.Add(identityColumn.PropertyInfo.Name, identityInt);
                identityColumn.PropertyInfo.SetValue(entity, identityInt, null);
            }
            else
            {
                connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
                keyValues.Add(column.PropertyInfo.Name, column.PropertyInfo.GetValue(entity, null));

            if (keyValues.Count == 1)
                return keyValues.First().Value;

            return keyValues;
        }

        public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var predicate = GetKeyPredicate(classMap, entity);
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Update(classMap, predicate, parameters);
            var dynamicParameters = new DynamicParameters();

            var columns = classMap.LinqPropertyMaps
                .Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity))
                .ToList();

            foreach (var property in ReflectionHelper.GetObjectValues(entity, columns))
            {
                var type = columns.Where(column => column.PropertyInfo.Name == property.Key)
                    .Select(column => column.Type).First();
                dynamicParameters.Add(property.Key, property.Value, type);
            }

            foreach (var parameter in parameters)
                dynamicParameters.Add(parameter.Key, parameter.Value);

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var predicate = GetKeyPredicate(classMap, entity);
            return Delete(connection, classMap, predicate, transaction, commandTimeout);
        }

        public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction,
            int? commandTimeout) where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var wherePredicate = GetPredicate(classMap, predicate);
            return Delete(connection, classMap, wherePredicate, transaction, commandTimeout);
        }

        public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort,
            IDbTransaction transaction, int? commandTimeout, bool buffered, int? topRecords, bool nolock)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var wherePredicate = GetPredicate(classMap, predicate);
            return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, buffered,
                topRecords, nolock);
        }

        public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout)
            where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var wherePredicate = GetPredicate(classMap, predicate);
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
            var dynamicParameters = new DynamicParameters();

            foreach (var parameter in parameters)
                dynamicParameters.Add(parameter.Key, parameter.Value);

            return
                (int)
                connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text)
                    .Single()
                    .Total;
        }

        private IEnumerable<T> GetList<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
            IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, int? topRecords,
            bool nolock) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Select(classMap, predicate, sort, parameters);

            if (topRecords.HasValue)
                sql = SqlGenerator.Configuration.Dialect.SelectLimit(sql, topRecords.Value);

            if (nolock)
                sql = SqlGenerator.Configuration.Dialect.SetNolock(sql);

            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
                dynamicParameters.Add(parameter.Key, parameter.Value);

            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        private bool Delete(IDbConnection connection, IClassMapper classMap, IPredicate predicate,
            IDbTransaction transaction, int? commandTimeout)
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Delete(classMap, predicate, parameters);
            var dynamicParameters = new DynamicParameters();

            foreach (var parameter in parameters)
                dynamicParameters.Add(parameter.Key, parameter.Value);

            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        private IPredicate GetPredicate(IClassMapper classMap, object predicate)
        {
            var wherePredicate = predicate as IPredicate;
            if (wherePredicate == null && predicate != null)
                wherePredicate = GetEntityPredicate(classMap, predicate);

            return wherePredicate;
        }

        private IPredicate GetIdPredicate(IClassMapper classMap, object id)
        {
            var isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
            var keys = classMap.LinqPropertyMaps.Where(p => p.KeyType != KeyType.NotAKey);
            IDictionary<string, object> paramValues = null;
            IList<IPredicate> predicates = new List<IPredicate>();
            if (!isSimpleType)
                paramValues = ReflectionHelper.GetObjectValues(id, classMap.LinqPropertyMaps);

            foreach (var key in keys)
            {
                var value = id;
                if (!isSimpleType)
                    value = paramValues[key.PropertyInfo.Name];

                var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

                var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                if (fieldPredicate == null)
                    throw new NullReferenceException("Unable to create instance of IFieldPredicate");

                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = key.PropertyInfo.Name;
                fieldPredicate.Value = value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                ? predicates[0]
                : new PredicateGroup
                {
                    Operator = GroupOperator.And,
                    Predicates = predicates
                };
        }

        private IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
        {
            var whereFields = classMap.LinqPropertyMaps
                .Where(p => p.KeyType != KeyType.NotAKey)
                .ToList();

            if (!whereFields.Any())
                throw new ArgumentException("At least one Key column must be defined.");

            IList<IPredicate> predicates = whereFields
                .Select(field => new FieldPredicate<T>
                {
                    Not = false,
                    Operator = Operator.Eq,
                    PropertyName = field.PropertyInfo.Name,
                    Value = field.PropertyInfo.GetValue(entity, null)
                })
                .Cast<IPredicate>()
                .ToList();

            return predicates.Count == 1
                ? predicates[0]
                : new PredicateGroup
                {
                    Operator = GroupOperator.And,
                    Predicates = predicates
                };
        }

        private IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
        {
            var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            IList<IPredicate> predicates = new List<IPredicate>();

            foreach (var kvp in ReflectionHelper.GetObjectValues(entity, classMap.LinqPropertyMaps))
            {
                var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                if (fieldPredicate == null)
                    throw new NullReferenceException("Unable to create instance of IFieldPredicate");

                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = kvp.Key;
                fieldPredicate.Value = kvp.Value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                ? predicates[0]
                : new PredicateGroup
                {
                    Operator = GroupOperator.And,
                    Predicates = predicates
                };
        }
    }
}