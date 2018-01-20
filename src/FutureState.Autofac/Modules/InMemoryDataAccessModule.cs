using System;
using Autofac;
using FutureState.Data;

namespace FutureState.Autofac.Modules
{
    /// <summary>
    ///     Registers in memory repositories, readers and units of work.
    /// </summary>
    public class InMemoryDataAccessModule : Module
    {
        /// <summary>
        ///     Registers a single instance in memory repository for any given entity type and an in memory session into a given
        ///     container.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(m => new CommitPolicyNoOp())
                .As<ICommitPolicy>();

            builder.Register(m => new InMemorySession())
                .As<ISession>()
                .SingleInstance();

            builder.Register(m => new InMemorySessionFactory())
                .As<ISessionFactory>()
                .SingleInstance();

            //in memory repositories
            builder.RegisterGeneric(typeof(InMemoryRepository<>))
                .AsSelf()
                .As(typeof(IRepository<>))
                .As(typeof(IRepositoryLinq<>))
                .As(typeof(ILinqReader<>))
                .As(typeof(IReader<>))
                .Named("Default", typeof(IRepository<>))
                .Named("Default", typeof(IRepositoryLinq<>))
                .Named("Default", typeof(ILinqReader<>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(InMemoryRepository<,>))
                .AsSelf()
                .As(typeof(IRepositoryLinq<,>))
                .As(typeof(IRepository<,>))
                .As(typeof(ILinqReader<,>))
                .Named("Default", typeof(IRepository<,>))
                .Named("Default", typeof(IRepositoryLinq<,>))
                .Named("Default", typeof(ILinqReader<,>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(KeyBinderFromAttributes<,>))
                .AsSelf()
                .As(typeof(IKeyBinder<,>))
                .SingleInstance();

            // use in memory key generator
            // override any defaults
            builder.RegisterGeneric(typeof(KeyGeneratorInMemory<,>))
                .AsSelf()
                .As(typeof(IKeyGenerator<,>))
                .SingleInstance();
        }

        public class KeyGeneratorInMemory<TEntity, TKey> :
            KeyGenerator<TEntity, TKey>
        {
            public KeyGeneratorInMemory() : base(GetKeyGeneratorFunction())
            {
            }

            static Func<TKey> GetKeyGeneratorFunction()
            {
                if (typeof(TKey) == typeof(int))
                {
                    int i = 0;

                    return () =>
                    {
                        i++;
                        object key = i;
                        return (TKey)key;
                    };
                }

                if (typeof(TKey) == typeof(short))
                {
                    short i = 0;

                    return () =>
                    {
                        i++;
                        object key = i;
                        return (TKey)key;
                    };
                }

                if (typeof(TKey) == typeof(long))
                {
                    long i = 0;

                    return () =>
                    {
                        i++;
                        object key = i;
                        return (TKey)key;
                    };
                }

                if (typeof(TKey) == typeof(Guid))
                {
                    return () =>
                    {
                        object key = Guid.NewGuid();
                        return (TKey)key;
                    };
                }

                if (typeof(TKey) == typeof(string))
                {
                    return () =>
                    {
                        object key = Guid.NewGuid().ToString();
                        return (TKey)key;
                    };
                }

                throw new NotSupportedException("Key type not supported.");
            }
        }
    }
}