using System.Collections.Generic;
using Xunit;

namespace FutureState.Common.Tests
{
    public class DeepCloneUtilTests
    {
        public class TestEntity1
        {
            public string Name { get; set; }

            public List<TestEntity2> Collection { get; set; }

            public TestEntity2 TestEntity2 { get; set; }
        }

        public class TestEntity2
        {
            public string Name { get; set; }
        }

        [Fact]
        public void WhenPerformingDeepCloneNestedObjectsClone()
        {
            var source = new TestEntity1
            {
                Name = "Name",
                Collection = new List<TestEntity2>
                {
                    new TestEntity2 {Name = "Name3"}
                },
                TestEntity2 = new TestEntity2 {Name = "Name2"}
            };

            var clone = DeepCloneUtil.DeepFieldClone(source);

            Assert.False(ReferenceEquals(clone, source));

            Assert.Equal(source.Name, clone.Name);
            Assert.Equal(source.Collection[0].Name, clone.Collection[0].Name);
        }
    }
}