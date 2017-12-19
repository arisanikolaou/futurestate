using System;
using FutureState.Reflection;
using Xunit;

namespace FutureState.Common.Tests
{
    public class DefaultMapperTests
    {
        [Fact]
        public void CanCopyOneEntityToAnother()
        {
            var source = new TestEntity()
            {
                Id = 1,
                Name = "Name"
            };

            var copy = new TestEntity2();

            source.MapTo(copy);

            Assert.Equal(source.Name, copy.Name);
            Assert.Equal(source.Id, copy.Id);
        }

        public class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TestEntity2 : TestEntity
        {
            public DateTime DateTime { get; set; }
        }
    }
}
