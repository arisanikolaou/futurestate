using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.Extensions.Linq.Predicates
{
    public abstract class BasePredicate : IBasePredicate
    {
        public abstract string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);

        public string PropertyName { get; set; }

        protected virtual string GetColumnName(Type entityType, ISqlGenerator sqlGenerator, string propertyName)
        {
            var map = sqlGenerator.Configuration.GetMap(entityType);
            if (map == null)
                throw new NullReferenceException($"Map was not found for {entityType}");

            var propertyMap = map.LinqPropertyMaps.SingleOrDefault(p => p.PropertyInfo.Name == propertyName);
            if (propertyMap == null)
                throw new NullReferenceException($"{propertyName} was not found for {entityType}");

            return sqlGenerator.GetColumnName(map, propertyMap, false);
        }
    }
}