using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Sql;
using FutureState.Data.Sql.Mappings;
using FutureState.Data.Sql.Tests.TestModels.RepositoryTests;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Sql.Tests
{
    [Story]
    public class RepositoryCanDoCrudStory
    {
        private string connectionString;
        private IDapperConfiguration config;
        private SessionFactory sessionFactory;
        private MyEntity[] _resultAfterInitialGetAll;
        private MyEntity[] _resultsAfterInserted;
        private MyEntity whereQueryByName;
        private MyEntity _entityQueriedByKey;
        private MyEntity _updatedResult;
        private MyEntity _deletedEntity;
        private MyEntity[] _resultsAfterDeleted;
        private IEnumerable<ProjectedItem> _projectedItem;

        protected void GivenANewTestSqlDbWithASingleEntity()
        {
            string dbServerName = LocalDbSetup.LocalDbServerName;
            string dbName = "FutureState.Data.Tests.TestModels.RepositoryTests.RepositoryDb";
            this.connectionString = $@"data source={dbServerName};initial catalog={dbName};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

            // delete any existing databases
            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, dbName);
            dbSetup.CreateLocalDb(true);

            // create db
            using (var repositoryDb = new RepositoryDb(connectionString))
            {
                repositoryDb.MyEntities.Add(new TestModels.RepositoryTests.MyEntity() { Id = 1, Name = "Name" });

                repositoryDb.SaveChanges();
            }

            // asser setup is complete
            using (var repositoryDb = new RepositoryDb(connectionString))
            {
                var result = repositoryDb.MyEntities.FirstOrDefault(m => m.Name == "Name");

                Assert.NotNull(result);
            }
        }

        protected void AndGivenADapperConfiguration()
        {
            // instance be application scope
            this.config = DapperConfiguration
                .Use()
                .UseSqlDialect(new SqlServerDialect());

            var classMapper = new CustomEntityMap<MyEntity>();
            classMapper.SetIdentityGenerated(m => m.Id);

            var classMappers = new List<IClassMapper>()
            {
                classMapper
            };

            classMappers.Each(n => config.Register(n));
        }

        protected void AndGivenAValidSessionFactory()
        {
            this.sessionFactory = new SessionFactory(connectionString, config);

            using (var session = sessionFactory.Create())
            {
                // test connection
                session.Dispose();
            }
        }

        protected void WhenExecutingCrudOperations()
        {
            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._resultAfterInitialGetAll = repo.GetAll().ToArray();

                Assert.True(_resultAfterInitialGetAll.Length > 0);

                repo.Insert(new MyEntity() { Name = "Name 2" });

                this._resultsAfterInserted = repo.GetAll().ToArray();

            }
        }

        protected void AndWhenQueryingByName()
        {
            using (var session = sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this.whereQueryByName = repo
                    .Where(m => m.Name == "Name 2")
                    .FirstOrDefault();
            }
        }

        protected void AndWhenGettingByKey()
        {
            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._entityQueriedByKey = repo.Get(1);
            }
        }

        protected void AndWhenProjecting()
        {
            using (var session = sessionFactory.Create())
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
            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                var result = repo.Get(1);
                result.Name = "Updated Name";

                repo.Update(result);
            }

            using (var session = sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._updatedResult = repo.Where(m => m.Name == "Updated Name").FirstOrDefault();
            }
        }

        protected void AndWhenDeleting()
        {
            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                repo.DeleteById(1);
            }

            using (var session = sessionFactory.Create())
            {
                var repo = new RepositoryLinq<MyEntity, int>(session);

                this._deletedEntity = repo.Get(1);
            }
        }

        protected void AndWhenDeletingAll()
        {
            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                repo.DeleteAll();
            }

            using (var session = sessionFactory.Create())
            {
                var repo = new Repository<MyEntity, int>(session);

                this._resultsAfterDeleted = repo.GetAll().ToArray();
            }
        }

        protected void ThenRepositoryShouldWork()
        {
            Assert.Equal(2, _resultsAfterInserted.Length);

            Assert.NotNull(_resultsAfterInserted.FirstOrDefault(m => m.Name == "Name 2"));

            Assert.NotNull(whereQueryByName);
            
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
