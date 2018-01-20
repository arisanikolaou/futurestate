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

namespace FutureState.Data.Sql.Tests.Repository
{
    [Story]
    public class RepositoryCanDoCrudStory
    {
        private string _conString;
        private IDapperConfiguration _config;
        private SessionFactory _sessionFactory;
        private MyEntity[] _resultAfterInitialGetAll;
        private MyEntity[] _resultsAfterInserted;
        private MyEntity _whereQueryByName;
        private MyEntity _entityQueriedByKey;
        private MyEntity _updatedResult;
        private MyEntity _deletedEntity;
        private MyEntity[] _resultsAfterDeleted;
        private IEnumerable<ProjectedItem> _projectedItem;
        private readonly DateTime _referencedate = new DateTime(2011, 1, 1, 9, 30, 15);
        public decimal _referenceNumber = 14.12m;

        protected void GivenANewTestSqlDbWithASingleEntity()
        {
            string dbServerName = LocalDbSetup.LocalDbServerName;
            string dbName = "RepositoryCanDoCrudStory";
            this._conString = $@"data source={dbServerName};initial catalog={dbName};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

            // delete any existing databases
            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, dbName);
            dbSetup.CreateLocalDb(true);

            // create db
            using (var repositoryDb = new TestModel(_conString))
            {
                repositoryDb.MyEntities
                    .Add(new MyEntity() { Id = 1, Name = "Name", Date = _referencedate, Money = _referenceNumber});

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

        protected void AndGivenADapperConfiguration()
        {
            // instance be application scope
            this._config = DapperConfiguration
                .Use()
                .UseSqlDialect(new SqlServerDialect());

            var classMapper = new CustomEntityMap<MyEntity>();
            classMapper.SetIdentityGenerated(m => m.Id);

            var classMappers = new List<IClassMapper>()
            {
                classMapper
            };

            classMappers.Each(n => _config.Register(n));
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

        protected void WhenExecutingCrudOperations()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._resultAfterInitialGetAll = repo.GetAll().ToArray();

                Assert.True(_resultAfterInitialGetAll.Length > 0);

                repo.Insert(new MyEntity() { Name = "Name 2", Date = _referencedate, Money = _referenceNumber });

                this._resultsAfterInserted = repo.GetAll().ToArray();

            }
        }

        protected void AndWhenQueryingByName()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._whereQueryByName = repo
                    .Where(m => m.Name == "Name 2")
                    .FirstOrDefault();
            }
        }

        protected void AndWhenGettingByKey()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._entityQueriedByKey = repo.Get(1);
            }
        }

        protected void AndWhenProjecting()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._projectedItem = repo.Select<ProjectedItem>(m => m.Id == 1);
            }
        }

        // has to be public
        public class ProjectedItem
        {
            public string Name { get; set; }
        }

        protected void AndWhenUpdating()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                var result = repo.Get(1);
                result.Name = "Updated Name";

                repo.Update(result);
            }

            using (var session = _sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._updatedResult = repo.Where(m => m.Name == "Updated Name").FirstOrDefault();
            }
        }

        protected void AndWhenDeleting()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                repo.DeleteById(1);
            }

            using (var session = _sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._deletedEntity = repo.Get(1);
            }
        }

        protected void AndWhenDeletingAll()
        {
            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                repo.DeleteAll();
            }

            using (var session = _sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._resultsAfterDeleted = repo.GetAll().ToArray();
            }
        }

        protected void ThenRepositoryReadWriteShouldWork()
        {
            Assert.Equal(2, _resultsAfterInserted.Length);

            Assert.NotNull(_resultsAfterInserted.FirstOrDefault(m => m.Name == "Name 2"));

            // check date loads
            Assert.Equal(_referencedate, _resultsAfterInserted.FirstOrDefault(m => m.Name == "Name 2")?.Date);

            // check number
            Assert.Equal(_referenceNumber, _resultsAfterInserted.FirstOrDefault(m => m.Name == "Name 2")?.Money);

            Assert.NotNull(_whereQueryByName);

            Assert.NotNull(_updatedResult);

            Assert.NotNull(_entityQueriedByKey);

            Assert.Null(_deletedEntity);

            Assert.Empty(_resultsAfterDeleted);

            Assert.Equal("Name", _projectedItem.Select(m => m.Name).FirstOrDefault());
        }

        [BddfyFact]
        protected void RepositoryCanDoCrud()
        {
            this.BDDfy();
        }
    }
}
