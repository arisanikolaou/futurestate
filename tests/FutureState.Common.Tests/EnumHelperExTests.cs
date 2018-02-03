using System;
using Xunit;

namespace FutureState.Common.Tests
{
    public class EnumHelperExTests
    {
        public enum Numbers
        {
            One,
            Two
        }

        [Fact]
        public void GetEnumTests()
        {
            var res = EnumHelperEx<Numbers>.GetEnum("One");

            Assert.Equal(Numbers.One, res);
        }

        [Fact]
        public void GetEnumTestsThrowsExceptionWhenAskedForNonExistingEnum()
        {
            Assert.Throws<NotSupportedException>(() => { EnumHelperEx<Numbers>.GetEnum("Three"); });
        }

        [Fact]
        public void GetEnumTests2()
        {
            var res = EnumHelperEx<Numbers>.GetEnumOrDefault("NonExisting");

            Assert.Equal(default(Numbers), res);
        }
    }
}