using Xunit;

namespace FutureState.Data.Tests
{
    public class PageResponseTests
    {
        [Fact]
        public void PageResponseShouldInitializeWithValidValues()
        {
            var col = new[] { 1, 2, 3 };
            var subject = new PageResponse<int>(col, 5);

            Assert.Equal(5,subject.TotalCount);

            Assert.Equal(col, subject.Items);
        }
    }
}
