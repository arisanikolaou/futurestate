using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Specifications.Tests
{
    [Story]
    public class DevsCanDefineSpecsForAnEntityStory
    {
        private Specification<TestClass> spec;
        private SpecResult resultValid;
        private SpecResult resultInvalid;

        public void GivenASpecification()
        {
            this.spec = new Specification<TestClass>((b) =>
            {
                if (string.IsNullOrWhiteSpace(b.Name))
                    return new SpecResult($"Test class {b.Id} must have an id.");
                return SpecResult.Success;
            }, "Name", "Some Description");
        }

        public void WhenTestingAValidEntity()
        {
            var validEntity = new TestClass() { Name = "Name" };
            this.resultValid = spec.Evaluate(validEntity);
        }

        public void WhenTestingAnInValidEntity()
        {
            var validEntity = new TestClass() { Name = null };
            this.resultInvalid = spec.Evaluate(validEntity);
        }

        public void ThenResultsShouldBeValid()
        {
            Assert.True(resultValid == SpecResult.Success);

            Assert.True(resultInvalid != SpecResult.Success);
        }

        public void AndThenInvalidSpecResultContainsDetailedErrorMessage()
        {
            Assert.True(resultInvalid.DetailedErrorMessage == $"Test class 0 must have an id.");
        }

        [BddfyFact]
        public void DevsCanDefineSpecsForAnEntity()
        {
            this.BDDfy();
        }

        public class TestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}