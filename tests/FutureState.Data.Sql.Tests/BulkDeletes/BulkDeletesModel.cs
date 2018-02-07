using System.ComponentModel.DataAnnotations;

namespace FutureState.Data.Sql.Tests.BulkDeletes
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class BulkDeletesModel : DbContext
    {
        public BulkDeletesModel(string conString)
            : base(conString)
        {
            this.Database.Initialize(true);
        }


        public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    public class MyEntity : IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}