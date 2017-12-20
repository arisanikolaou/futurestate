using Xunit;

namespace FutureState.Common.Tests
{
    public class ObjectHelperTests
    {
        [Fact]
        public void CanCompareTwoEquivalentEntities()
        {
            var equals = ObjectHelper.AreValuesEqual(new TestEntity(), new TestEntity());

            Assert.True(equals);
        }

        [Fact]
        public void CanCompareTwoEquivalentEntities2()
        {
            var equals = ObjectHelper.AreValuesEqual(new TestEntity() {Id = 1}, new TestEntity());

            Assert.False(equals);
        }

        [Fact]
        public void CanCompareTwoEquivalentEntities3()
        {
            var equals = ObjectHelper.AreValuesEqual(new TestEntity() { Id = 1 }, new TestEntity() {Id = 1});

            Assert.True(equals);
        }

        public class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
