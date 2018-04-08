using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Sql;
using FutureState.Data.Sql.Mappings;
using FutureState.Data.Sql.Tests.BulkDeletes;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Sql.Tests
{ 
    [Story]
    public class UnitOfWorkBulkDeletesStory
    {
        private UnitOfWorkLinq<MyEntity, int> _unitOfWork;
        private string _dbName;
        private string _conString;

        protected void GivenASqlDatabaseWithData()
        {
            _dbName = GetType().Name;

            var dbServerName = LocalDbSetup.LocalDbServerName;
            if (string.IsNullOrWhiteSpace(_dbName))
                throw new InvalidOperationException();

            _conString =
                $@"data source={dbServerName};initial catalog={
                        _dbName
                    };integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

            // delete any existing databases
            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, _dbName);
            dbSetup.CreateLocalDb(true);

            using (var db = new BulkDeletesModel(_conString))
            {
                db.Database.CreateIfNotExists();

                for (int i = 0; i < 10; i++)
                    db.MyEntities.Add(new MyEntity() { Id = i, Name = "Name" + i });

                db.SaveChanges();
            }
        }

        protected void AndGivenAUnitOfWork()
        {
            // instance be application scope
            var config = DapperConfiguration
                .Use()
                .UseSqlDialect(new SqlServerDialect());

            var classMapper = new CustomEntityMap<MyEntity>();
            classMapper.SetIdentityGenerated(m => m.Id);

            var classMappers = new List<IClassMapper>
            {
                classMapper
            };

            classMappers.Each(m => config.Register(m));

            // only one supported for the time being
            var tranCommitPolicy = new CommitPolicyNoOp();

            this._unitOfWork = new UnitOfWorkLinq<MyEntity, int>(
                session => new RepositoryLinq<MyEntity, int>(session),
                new SessionFactory(_conString, config),
                tranCommitPolicy);
        }

        protected void WhenBulkDeleting()
        {
            using (_unitOfWork.Open())
            {
                _unitOfWork.EntitySet.BulkDeleter.Delete(m => m.Id > 5);
                _unitOfWork.Commit();
            }
        }

        protected void ThenAllResultsShouldBeBulkDeleted()
        {
            using (var db = new BulkDeletesModel(_conString))
                Assert.Equal(5, db.MyEntities.Count());
        }


       [BddfyFact]
       public void UnitOfWorkBulkDeletes()
        {
            this.BDDfy();
        }
    }
}
