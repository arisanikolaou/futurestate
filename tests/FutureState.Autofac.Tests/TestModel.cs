using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using FutureState.Data;

namespace FutureState.Autofac.Tests
{
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
        [StringLength(200)] public string Name { get; set; }

        [Key] public int Id { get; set; }
    }

    public class Address : IEntityMutableKey<Guid>
    {
        [StringLength(200)] public string Name { get; set; }

        [Key] public Guid Id { get; set; }
    }
}