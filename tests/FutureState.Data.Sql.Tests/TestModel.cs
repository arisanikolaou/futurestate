namespace FutureState.Data.Sql.Tests
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;

    public class TestModel : DbContext
    {

        public TestModel(string conString)
            : base(conString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TestModel>());
        }

        public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    public class MyEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}