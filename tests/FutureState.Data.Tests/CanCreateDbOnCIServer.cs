using System;
using System.Linq;
using FutureState.Data.Tests.TestModels.RepositoryTests;
using Xunit;

namespace FutureState.Data.Tests
{
    public class CanCreateDbOnCIServer
    {
        [Fact]
        public void CreateLocalDb()
        {
            // setup
            string dbServerName = LocalDbSetup.LocalDbServerName;
            string dbName = "FutureState.Data.Tests.TestModels.RepositoryTests.RepositoryDb";
            string connectionString = $@"data source={dbServerName};initial catalog={dbName};integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, dbName);
            dbSetup.CreateLocalDb(true);

            // act
            using (var repositoryDb = new RepositoryDb(connectionString))
            {
                repositoryDb.MyEntities.Add(new MyEntity() {Id = 1, Name = "Name"});

                repositoryDb.SaveChanges();
            }

            // assert
            using (var repositoryDb = new RepositoryDb(connectionString))
            {
                var result  = repositoryDb.MyEntities.FirstOrDefault(m => m.Name == "Name");

                Assert.NotNull(result);
            }
        }
    }
}
