using Xunit;

namespace FutureState.Data.Tests
{
    public class ExpressionKeyBinderTests
    {
        public class TestEntity
        {
            public string Key { get; set; }

            public string Id { get; set; }
        }

        [Fact]
        public void CanGetKeyFromEntity()
        {
            var subject = new KeyBinder<TestEntity, string>(entity => entity.Key, (e, k) => e.Key = k);

            var key = subject.Get(new TestEntity {Key = "Key1"});

            Assert.Equal("Key1", key);
        }


        [Fact]
        public void CanSetKeyOnEntity()
        {
            var subject = new KeyBinder<TestEntity, string>(entity => entity.Key, (e, key) => e.Key = key);

            var testEntity = new TestEntity();
            subject.Set(testEntity, "Key2");

            Assert.Equal("Key2", testEntity.Key);
        }
    }
}