using FluentAssertions;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Example.XUnit.Tests
{
    [Story(AsA = "As a developer using the Api", IWant = @"I want to test Bdd Tests", StoryUri = "So that..")]
    public class Story
    {
        private int _number;

        public void GivenANumber()
        {
            _number = 1;
        }

        public void WhenAddingANumber()
        {
            _number = _number + 1;
        }

        public void ThenResultShouldBeValid()
        {
            _number.Should().Be(2);
        }

        [BddfyFact()]
        public void MyStory()
        {
            this.BDDfy("Developer using the api.");
        }
    }
}
