using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Data.Sql.Tests.UnitOfWork
{
    public class TestEntity : IEntity<int>
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public decimal Money { get; set; }
        [Key] public int Id { get; set; }
    }
}