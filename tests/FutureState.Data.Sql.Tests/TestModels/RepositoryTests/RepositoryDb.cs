using System.Data.Entity;

namespace FutureState.Data.Sql.Tests.TestModels.RepositoryTests
{
    public class RepositoryDb : DbContext
    {
        public RepositoryDb()
            : base("name=RepositoryDb")
        {
        }

        public RepositoryDb(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    public class MyEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}