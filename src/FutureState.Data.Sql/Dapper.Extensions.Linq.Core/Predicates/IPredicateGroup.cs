using Dapper.Extensions.Linq.Core.Enums;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }
}