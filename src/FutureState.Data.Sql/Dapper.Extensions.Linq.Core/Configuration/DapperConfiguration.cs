using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Sql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public class DapperConfiguration : IDapperConfiguration
    {
        private readonly ConcurrentDictionary<Type, IClassMapper> _classMaps =
            new ConcurrentDictionary<Type, IClassMapper>();

        public readonly List<Assembly> Assemblies2;

        private DapperConfiguration()
        {
            ContainerCustomisations = new ContainerCustomisations();
            Assemblies2 = new List<Assembly>();
        }

        public IContainerCustomisations ContainerCustomisations { get; }

        public ISqlDialect Dialect { get; private set; }

        /// <summary>
        ///     Changes the <see cref="ISqlDialect" />.
        /// </summary>
        /// <param name="dialect"></param>
        /// <returns></returns>
        public IDapperConfiguration UseSqlDialect(ISqlDialect dialect)
        {
            Dialect = dialect;
            return this;
        }

        public IDapperConfiguration Build()
        {
            if (Dialect == null)
                throw new NullReferenceException("SqlDialect has not been set. Call UseSqlDialect().");

            return this;
        }

        public IClassMapper GetMap(Type entityType)
        {
            IClassMapper map;

            if (_classMaps.TryGetValue(entityType, out map))
                return map;

            throw new NotSupportedException($"Entity type {entityType.FullName} is not supported.");
        }

        public bool Register(IClassMapper mapper)
        {
            if (!IsClassMapped(mapper.EntityType))
                return _classMaps.TryAdd(mapper.EntityType, mapper);
            return
                false;
        }

        public bool IsClassMapped(Type type)
        {
            return _classMaps.ContainsKey(type);
        }

        public IClassMapper GetMap<T>() where T : class
        {
            return GetMap(typeof(T));
        }

        /// <summary>
        ///     Creates a Dapper configuration with default static <see cref="IConnectionStringProvider" /> and
        ///     <see cref="IDapperSessionContext" /> per thread.
        /// </summary>
        /// <returns></returns>
        public static IDapperConfiguration Use()
        {
            return new DapperConfiguration();
        }
    }
}