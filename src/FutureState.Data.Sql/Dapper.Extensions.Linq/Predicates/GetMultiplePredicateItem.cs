using Dapper.Extensions.Linq.Core.Predicates;
using System;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Predicates
{
    public class GetMultiplePredicateItem : IGetMultiplePredicateItem
    {
        public object Value { get; set; }
        public Type Type { get; set; }
        public IList<ISort> Sort { get; set; }
    }
}