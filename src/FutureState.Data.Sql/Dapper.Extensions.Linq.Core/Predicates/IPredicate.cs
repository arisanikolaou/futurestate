using Dapper.Extensions.Linq.Core.Sql;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IPredicate
    {
        string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);
    }
}