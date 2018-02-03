using Dapper.Extensions.Linq.Core.Sql;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Predicates
{
    public class BetweenPredicate<T> : BasePredicate, IBetweenPredicate
        where T : class
    {
        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName);
            var propertyName1 = parameters.SetParameterName(PropertyName, Value.Value1,
                sqlGenerator.Configuration.Dialect.ParameterPrefix);
            var propertyName2 = parameters.SetParameterName(PropertyName, Value.Value2,
                sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1}BETWEEN {2} AND {3})", columnName, Not ? "NOT " : string.Empty, propertyName1,
                propertyName2);
        }

        public BetweenValues Value { get; set; }

        public bool Not { get; set; }
    }
}