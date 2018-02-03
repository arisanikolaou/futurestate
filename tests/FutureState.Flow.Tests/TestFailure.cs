using Xunit;

namespace FutureState.Flow.Tests
{
    public class TestFailure
    {
        [Fact]
        public void AlwaysFail()
        {
            // test pre-push hook
             
            Assert.False(true);
        }
    }
}
