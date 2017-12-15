using System;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public interface IDapperConfiguration
    {
        ISqlDialect Dialect { get; }

        IDapperConfiguration UseSqlDialect(ISqlDialect dialect);

        bool Register(IClassMapper mapper);

        bool IsClassMapped(Type type);

        IDapperConfiguration Build();

        IClassMapper GetMap(Type entityType);

        IClassMapper GetMap<T>() where T : class;
    }
}