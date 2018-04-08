using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FutureState.Data.Tests
{
    public class AttributeKeyBinderTests
    {
        public class TestEntity
        {
            public int Id { get; set; }

            [Key] public string Key { get; set; }
        }

        [Fact]
        public void CanGetAndSetEntitiesWithKeyAttribute()
        {
            var subject = new KeyBinderFromAttributes<TestEntity, string>();

            var testEntity = new TestEntity { Key = "Key" };

            var key = subject.Get(testEntity);

            Assert.Equal("Key", key);

            subject.Set(testEntity, "Key2");

            key = subject.Get(testEntity);

            Assert.Equal("Key2", key);
        }
    }
}