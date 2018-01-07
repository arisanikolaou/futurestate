namespace FutureState.Data.Tests.TestModels.RepositoryTests
{
    using System.Data.Entity;

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