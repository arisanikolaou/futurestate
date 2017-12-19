using System;
using System.ComponentModel.DataAnnotations;
using FutureState.Data;

namespace FutureState.Autofac.Tests
{
    using System.Data.Entity;

    public class TestModel : DbContext
    {
        public TestModel(string conString)
            : base(conString)
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<TestModel>());
        }

        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
    }

    public class Contact : IEntityMutableKey<int>
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Name { get; set; }
    }

    public class Address : IEntityMutableKey<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string Name { get; set; }
    }
}