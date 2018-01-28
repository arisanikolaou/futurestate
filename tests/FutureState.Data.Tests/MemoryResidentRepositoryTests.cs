using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Tests
{
    public class MemoryResidentRepositoryTests
    {
        private int _allItemsCount;
        private int _idItemsCount;
        private TestEntity _insertedEntity;
        private InMemoryRepository<TestEntity, int> subject;

        internal void GivenAnInMemoryDb()
        {
            var entityKeyBinder = new KeyBinder<TestEntity, int>(
                e => e.Id,
                (e, k) => e.Id = k);

            var keyGenerator = new KeyGenerator<TestEntity, int>(() => ++_idItemsCount);

            var entityIdProvider = new KeyProvider<TestEntity, int>(
                keyGenerator,
                entityKeyBinder);

            var list = new List<TestEntity>
            {
                new TestEntity {Id = 0, Name = "Name"}
            };

            subject = new InMemoryRepository<TestEntity, int>(
                entityIdProvider, entityKeyBinder, list);
        }

        internal void WhenQueringAllItems()
        {
            _allItemsCount = subject.GetAll().Count();
        }

        internal void AndWhenAddingNewItems()
        {
            subject.Insert(new TestEntity {Name = "Name2"});

            _insertedEntity = subject.Where(m => m.Name == "Name2").FirstOrDefault();
        }

        internal void ThenShouldBeAbleToQueryAllItems()
        {
            Assert.Equal(1, _allItemsCount);
        }

        internal void AndThenInsertedItemsShouldHaveIdAssigned()
        {
            Assert.Equal(1, _insertedEntity.Id);
        }

        [BddfyFact]
        public void CanGetAndSetDataIntoRepository()
        {
            this.BDDfy();
        }

        public class TestEntity
        {
            [Key] public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}