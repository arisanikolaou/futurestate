using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Sql;
using FutureState.Data.Sql.Mappings;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Sql.Tests.UnitOfWork
{
    [Story]
    public class UnitOfWorkManagesTransactionsNoOpCommitPolicyStory : UnitOfWorkManagesTransactionsStoryBase
    {
        [BddfyFact]
        public void UnitOfWorkManagesTransactionsNoOpCommitPolicy()
        {
            this._commitPolicy = new CommitPolicyNoOp(); // don't rely on sql transaction
            this._dbName = "UnitOfWorkManagesTransactionsNoOpCommitPolicy";

            this.BDDfy();
        }
    }

    [Story]
    public class UnitOfWorkManagesTransactionsSqlTransactionalCommitPolicyStory : UnitOfWorkManagesTransactionsStoryBase
    {
        [BddfyFact]
        public void UnitOfWorkManagesTransactionsSqlTransactionalCommitPolicy()
        {
            this._commitPolicy = new CommitPolicy(); // always create sql transaction
            this._dbName = "UnitOfWorkManagesTransactionsSqlTransactionalCommitPolicy";

            this.BDDfy();
        }
    }

    [Story]
    public class UnitOfWorkManagesTransactionsWithInMemRepositoryStory : UnitOfWorkManagesTransactionsStoryBase
    {
        private InMemoryRepository<TestEntity, int> _repository;

        [BddfyFact]
        public void UnitOfWorkManagesTransactionsWithInMemRepository()
        {
            this._dbName = "UnitOfWorkManagesTransactionsWithInMemRepository";

            this.BDDfy();
        }

        protected override IRepositoryLinq<TestEntity, int> GetRepository(ISession session)
        {
            if (_repository == null)
            {
                int i = 1; // todo: should be auto generated from class mappers

                _repository = new InMemoryRepository<TestEntity, int>(
                    new KeyProvider<TestEntity, int>(
                        new KeyGenerator<TestEntity, int>(() => i++)),
                    new KeyBinder<TestEntity, int>(m => m.Id, 
                        (entity, entityId) => entity.Id = entityId),
                    new List<TestEntity>());

            }

            return _repository;
        }
    }

    public abstract class UnitOfWorkManagesTransactionsStoryBase
    {
        private string _conString;
        private static readonly IDapperConfiguration _config;
        private SessionFactory _sessionFactory;
        private readonly DateTime _referencedate = new DateTime(2011, 1, 1, 9, 30, 15);
        protected decimal _referenceNumber = 14.12m;
        private UnitOfWork<TestEntity, int> _db;
        private TestEntity _insertedEntity;
        private TestEntity _insertedEntityNotCommited;
        private TestEntity _updatedEntity;
        private TestEntity[] _deletedEntities;
        protected ICommitPolicy _commitPolicy;
        protected string _dbName;

        static UnitOfWorkManagesTransactionsStoryBase()
        {
            // can only be configured once per entity
            _config = DapperConfiguration
                .Use()
                .UseSqlDialect(new SqlServerDialect());

            var classMapper = new CustomEntityMap<TestEntity>();
            classMapper.SetIdentityGenerated(m => m.Id);

            var classMappers = new List<IClassMapper>()
            {
                classMapper
            };

            classMappers.Each(n => _config.Register(n));
        }

        protected void GivenANewTestSqlDbWithASingleEntity()
        {
            string dbServerName = LocalDbSetup.LocalDbServerName;
            if (string.IsNullOrWhiteSpace(_dbName))
                throw new InvalidOperationException();

            this._conString = $@"data source={dbServerName};initial catalog={_dbName};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

            // delete any existing databases
            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, _dbName);
            dbSetup.CreateLocalDb(true);

            // create db
            using (var repositoryDb = new TestModel(_conString))
            {
                repositoryDb.MyEntities
                    .Add(new TestEntity() { Id = 1, Name = "Name", Date = _referencedate, Money = _referenceNumber });

                repositoryDb.SaveChanges();
            }

            // asser setup is complete
            using (var repositoryDb = new TestModel(_conString))
            {
                var result = repositoryDb.MyEntities
                    .FirstOrDefault(m => m.Name == "Name");

                Assert.NotNull(result);
            }
        }

        protected void AndGivenAValidSessionFactory()
        {
            this._sessionFactory = new SessionFactory(_conString, _config);

            using (var session = _sessionFactory.Create())
            {
                // test connection
                session.Dispose();
            }
        }

        protected void AndGivenAUnitOfWork()
        {
            this._db = new UnitOfWork<TestEntity, int>(
                GetRepository,
                _sessionFactory,
                _commitPolicy);
        }

        protected virtual IRepositoryLinq<TestEntity, int> GetRepository(ISession session)
        {
            return new RepositoryLinq<TestEntity, int>(session);
        }

        protected void WhenInsertingViaUnitOfWork()
        {
            using (_db.Open())
            {
                _db.EntitySet.Writer.Insert(new TestEntity()
                {
                    Date = _referencedate,
                    Money = _referenceNumber,
                    Name = "Name"
                });

                TestEntity testEntity2;
                _db.EntitySet.Writer.Insert(testEntity2 = new TestEntity()
                {
                    Date = _referencedate,
                    Money = _referenceNumber,
                    Name = "Name 2"
                });

                _db.EntitySet.Writer.Insert(testEntity2);

                testEntity2.Name = "Name 3";

                _db.EntitySet.Writer.Update(testEntity2);

                // save changes
                _db.Commit();
            }

            using (_db.Open())
            {
                this._insertedEntity = _db.EntitySet.Reader.Get(1);

                this._updatedEntity = _db.EntitySet
                    .Reader.GetAll().FirstOrDefault(m => m.Name == "Name 3");
            }
        }

        protected void AndWhenInsertingButNotCommitting()
        {
            using (_db.Open())
            {
                _db.EntitySet.Writer.Insert(new TestEntity()
                {
                    Date = _referencedate,
                    Money = _referenceNumber,
                    Name = "Not commited"
                });

                // don't save changes
                // 2 would be the next entity
                this._insertedEntityNotCommited = _db.EntitySet
                    .Reader.GetAll().FirstOrDefault(m => m.Name == "Not commited");
            }
        }

        protected void AndWhenDeletingAllViaUnitOfWork()
        {
            using (_db.Open())
            {
                _db.EntitySet.Writer.DeleteAll();

                _db.Commit();
            }

            using (_db.Open())
            {
                _deletedEntities = _db.EntitySet.Reader.GetAll().ToArray();
            }
        }

        protected void ThenShouldBeAbleToInsertAndUpdateEntities()
        {
            Assert.NotNull(_insertedEntity);
            Assert.Null(_insertedEntityNotCommited);
            Assert.NotNull(_updatedEntity);
            Assert.Empty(_deletedEntities);
        }

        protected void ThenShouldNotBeAbleToReadWhenSessionIsNotOpen()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                // user has to explicitly call Open
                this._insertedEntityNotCommited = _db.EntitySet.Reader.Get(1);
            });
        }

        protected void ThenShouldNotBeAbleToOpen2ConsecutiveSessions()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (_db.Open())
                {
                    using (_db.Open()) // should raise an exception
                    {
                        this._insertedEntityNotCommited = _db.EntitySet.Reader.Get(2);
                    }
                }
            });
        }
    }
}
