using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace FutureState.Data.Sql.Tests.Repository
{
    public class TestModel : DbContext
    {
        public TestModel(string conString)
            : base(conString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TestModel>());
        }

        public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    public class MyEntity : IEntity<int>
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public decimal Money { get; set; }
        [Key] public int Id { get; set; }
    }
}