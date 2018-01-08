using System.Data.Entity;
using FutureState.Data.Sql.Tests.Repository;

namespace FutureState.Data.Sql.Tests.UnitOfWork
{
    public class TestModel : DbContext
    {
        public TestModel(string conString)
            : base(conString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<Repository.TestModel>());
        }

        public virtual DbSet<TestEntity> MyEntities { get; set; }
    }
}