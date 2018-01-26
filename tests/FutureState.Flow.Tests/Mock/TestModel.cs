using System.ComponentModel.DataAnnotations.Schema;

namespace FutureState.Flow.Tests.Mock
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;

    public class TestModel : DbContext
    {
        public TestModel()
            : base("name=TestModel")
        {
            this.Database.Initialize(true);
        }

        public TestModel(string conString)
            : base(conString)
        {
            this.Database.Initialize(true);
        }


        public virtual DbSet<Contact> Contacts { get; set; }


        public virtual DbSet<Address> Addresses { get; set; }
    }

    public class Contact
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ContactId { get; set; }

        public string StreetName { get; set; }
    }
}