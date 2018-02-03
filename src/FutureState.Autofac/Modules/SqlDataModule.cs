using Autofac;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Sql;
using FutureState.Data;
using FutureState.Data.Sql;
using System.Collections.Generic;

namespace FutureState.Autofac.Modules
{
    public class SqlDataModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //register dapper configuration
            builder.Register(m =>
                {
                    var classMappers = m.Resolve<IEnumerable<IClassMapper>>();

                    // instance be application scope
                    var config = DapperConfiguration
                        .Use()
                        .UseSqlDialect(new SqlServerDialect());

                    classMappers.Each(n => config.Register(n));

                    return config.Build();
                })
                .As<IDapperConfiguration>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .PreserveExistingDefaults();

            //regiser session factory
            builder.Register(m =>
                {
                    var componentContext = m.Resolve<IComponentContext>();

                    return new SessionFactory(ConnectionString, componentContext.Resolve<IDapperConfiguration>());
                })
                .AsSelf()
                .AsImplementedInterfaces();

            // guid key'ed entities
            builder.RegisterGeneric(typeof(RepositoryLinq<>))
                .AsSelf()
                .As(typeof(IRepositoryLinq<>))
                .As(typeof(IRepository<>))
                .As(typeof(ILinqReader<>))
                .Named("Default", typeof(IRepositoryLinq<>))
                .Named("Default", typeof(IRepository<>))
                .Named("Default", typeof(ILinqReader<>));

            // generic key
            builder.RegisterGeneric(typeof(RepositoryLinq<,>))
                .AsSelf()
                .As(typeof(IRepositoryLinq<,>))
                .As(typeof(ILinqReader<,>))
                .As(typeof(IRepository<,>))
                .Named("Default", typeof(IRepositoryLinq<,>))
                .Named("Default", typeof(IRepository<,>))
                .Named("Default", typeof(ILinqReader<,>));

            // use transactions
            builder.Register(m => new CommitPolicy())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}