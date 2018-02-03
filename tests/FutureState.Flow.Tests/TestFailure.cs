using Xunit;

namespace FutureState.Flow.Tests
{
    public class TestFailure
    {
        [Fact]
        public void AlwaysFail()
        {
            Assert.False(true);
        }
    }
}
