using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extensions.Linq.Sql
{
    public class SqlGeneratorImpl : ISqlGenerator
    {
        public SqlGeneratorImpl(IDapperConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IDapperConfiguration Configuration { get; }

        public string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort,
            IDictionary<string, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            var stringBuilder = new StringBuilder(
                $"SELECT {BuildSelectColumns(classMap)} FROM {GetTableName(classMap)}");
            if (predicate != null)
                stringBuilder.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));

            if (sort != null && sort.Any())
                stringBuilder.Append(" ORDER BY ")
                    .Append(
                        sort.Select(
                                s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC"))
                            .AppendStrings());

            var sql = stringBuilder.ToString();

            return sql;
        }

        public string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page,
            int resultsPerPage, IDictionary<string, object> parameters)
        {
            if (sort == null || !sort.Any())
                throw new ArgumentNullException(nameof(sort), "Sort cannot be null or empty.");

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var innerSql = new StringBuilder(
                $"SELECT {BuildSelectColumns(classMap)} FROM {GetTableName(classMap)}");

            if (predicate != null)
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));

            var orderBy =
                sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC"))
                    .AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            var sql = Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);

            return sql;
        }

        public string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult,
            int maxResults, IDictionary<string, object> parameters)
        {
            if (sort == null || !sort.Any())
                throw new ArgumentNullException("sort", "Sort cannot be null or empty.");

            if (parameters == null)
                throw new ArgumentNullException("parameters");

            var innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap),
                GetTableName(classMap)));
            if (predicate != null)
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));

            var orderBy =
                sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC"))
                    .AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            var sql = Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);

            return sql;
        }

        public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            var stringBuilder = new StringBuilder(string.Format("SELECT COUNT(*) AS {0}Total{1} FROM {2}",
                Configuration.Dialect.OpenQuote,
                Configuration.Dialect.CloseQuote,
                GetTableName(classMap)));
            if (predicate != null)
                stringBuilder.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));

            var sql = stringBuilder.ToString();

            return sql;
        }

        public string Insert(IClassMapper classMap)
        {
            var columns = classMap.LinqPropertyMaps
                .Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity))
                .ToList();

            if (!columns.Any())
                throw new ArgumentException("No columns were mapped.");

            var columnNames = columns.Select(p => GetColumnName(classMap, p, false));
            var parameters = columns.Select(p => Configuration.Dialect.ParameterPrefix + p.PropertyInfo.Name);

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                GetTableName(classMap),
                columnNames.AppendStrings(),
                parameters.AppendStrings());

            return sql;
        }

        public string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var columns = classMap.LinqPropertyMaps
                .Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity))
                .ToList();

            if (!columns.Any())
                throw new ArgumentException("No columns were mapped.");

            var setSql =
                columns.Select(
                    p =>
                        string.Format("{0} = {1}{2}", GetColumnName(classMap, p, false),
                            Configuration.Dialect.ParameterPrefix, p.PropertyInfo.Name));

            var sql = string.Format("UPDATE {0} SET {1} WHERE {2}",
                GetTableName(classMap),
                setSql.AppendStrings(),
                predicate.GetSql(this, parameters));

            return sql;
        }

        public string Delete(IClassMapper classMap, IPredicate predicate = null,
            IDictionary<string, object> parameters = null)
        {
            var stringBuilder = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(classMap)));

            if (predicate != null && parameters != null)
                stringBuilder.Append(" WHERE ").Append(predicate.GetSql(this, parameters));

            var sql = stringBuilder.ToString();

            return sql;
        }

        public string IdentitySql(IClassMapper classMap)
        {
            return Configuration.Dialect.GetIdentitySql(GetTableName(classMap));
        }

        public virtual string GetTableName(IClassMapper map)
        {
            return Configuration.Dialect.GetTableName(map.SchemaName, map.TableName, null);
        }

        public virtual string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
        {
            string alias = null;
            if (property.ColumnName != property.PropertyInfo.Name && includeAlias)
                alias = property.PropertyInfo.Name;

            return Configuration.Dialect.GetColumnName(GetTableName(map), property.ColumnName, alias);
        }

        public virtual string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
        {
            var propertyMap =
                map.LinqPropertyMaps.SingleOrDefault(
                    p => p.PropertyInfo.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
                throw new ArgumentException($"Could not find '{propertyName}' in Mapping.");

            return GetColumnName(map, propertyMap, includeAlias);
        }

        public bool SupportsMultipleStatements()
        {
            return Configuration.Dialect.SupportsMultipleStatements;
        }

        public virtual string BuildSelectColumns(IClassMapper classMap)
        {
            var columns = classMap.LinqPropertyMaps
                .Where(p => !p.Ignored)
                .Select(p => GetColumnName(classMap, p, true));
            return columns.AppendStrings();
        }
    }
}