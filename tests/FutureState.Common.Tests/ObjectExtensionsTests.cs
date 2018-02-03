using Xunit;

namespace FutureState.Common.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void DoesWithTest()
        {
            bool hitLine = false;

            var result = new DomainObject()
            {
                Name = "Name"
            }
                .Do(m =>
                {
                    hitLine = true;
                })
                .With(m => m.Name)
                .Exists();

            Assert.True(result);
            Assert.True(hitLine);
        }

        [Fact]
        public void ReturnsAfterWithTests()
        {
            var result = new DomainObject()
            {
                Name = "Name"
            }
                .With(m => m.Name)
                .Return(m => m[0]);

            Assert.Equal('N', result);
        }

        public class DomainObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}