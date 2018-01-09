using Xunit;

namespace FutureState.Common.Tests
{
    public class EnumHelperTests
    {
        public enum Numbers
        {
            One,
            Two
        }

        [Fact]
        public void CanParseEnumValueFromLabel()
        {
            var result = EnumHelper.GetEnumOrNull<Numbers>("One");

            Assert.Equal("One", result.ToString());

            var result2 = EnumHelper.GetEnumOrNull<Numbers>("One2");
            Assert.Null(result2);
        }

        [Fact]
        public void FailsToPareIfLabelIsDoesNotMatchEnum()
        {
            var result2 = EnumHelper.GetEnumOrNull<Numbers>("One2");
            Assert.Null(result2);
        }

    }
}