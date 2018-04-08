using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extensions.Linq.Predicates
{
    public class FieldPredicate<T> : ComparePredicate, IFieldPredicate
        where T : class
    {
        public object Value { get; set; }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName);
            if (Value == null)
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);

            if (Value is IEnumerable && !(Value is string))
            {
                if (Operator != Operator.Eq)
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");

                var @params = new List<string>();
                foreach (var value in (IEnumerable)Value)
                {
                    var valueParameterName = parameters.SetParameterName(PropertyName, value,
                        sqlGenerator.Configuration.Dialect.ParameterPrefix);
                    @params.Add(valueParameterName);
                }

                var paramStrings = @params.Aggregate(new StringBuilder(),
                    (sb, s) => sb.Append((sb.Length != 0 ? ", " : string.Empty) + s), sb => sb.ToString());
                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, paramStrings);
            }

            var parameterName = parameters.SetParameterName(PropertyName, Value,
                sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), parameterName);
        }
    }
}