
using Xunit;

namespace FutureState.Common.Tests
{
    public class EnumHelperTests
    {
        [Fact]
        public void CanParseEnumValueFromLabel()
        {
            Numbers? result = EnumHelper.GetEnumOrNull<Numbers>("One");

            Assert.Equal("One", result.ToString());

            Numbers? result2 = EnumHelper.GetEnumOrNull<Numbers>("One2");
            Assert.Null(result2);
        }

        [Fact]
        public void FailsToPareIfLabelIsDoesNotMatchEnum()
        {
            Numbers? result2 = EnumHelper.GetEnumOrNull<Numbers>("One2");
            Assert.Null(result2);
        }

        public enum Numbers
        {
            One,
            Two
        }
    }
}
