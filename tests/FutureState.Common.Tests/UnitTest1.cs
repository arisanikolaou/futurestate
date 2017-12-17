using FluentAssertions;
using Xunit;

namespace Example.xUnit.Tests
{
    public class UnitTest1
    {
        [Trait("Category","Sample")]
        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        public void Add(int number,bool expectedresult)
        {
            if(expectedresult)
                2.Should().Be(number);
            else
                2.Should().NotBe(number);
        }

        [Fact]
        public void Add2()
        {
            Assert.True(true);
        }
    }
}
