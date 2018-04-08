using Dapper.FluentMap.Dommel.Mapping;
using System;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Mapper
{
    public interface IClassMapper<T> : IClassMapper where T : class
    {
    }

    public interface IClassMapper : IDommelEntityMap
    {
        string SchemaName { get; }
        IList<IPropertyMap> LinqPropertyMaps { get; }
        Type EntityType { get; }
    }
}