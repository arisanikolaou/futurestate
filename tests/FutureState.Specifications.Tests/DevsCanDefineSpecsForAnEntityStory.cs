﻿using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Specifications.Tests
{
    [Story]
    public class DevsCanDefineSpecsForAnEntityStory
    {
        private SpecResult _resultInvalid;
        private SpecResult _resultValid;
        private Specification<TestClass> _spec;

        protected void GivenASpecification()
        {
            _spec = new Specification<TestClass>(b =>
            {
                if (string.IsNullOrWhiteSpace(b.Name))
                    return new SpecResult($"Test class {b.Id} must have an id.");
                return SpecResult.Success;
            }, "Name", "Some Description");
        }

        protected void WhenTestingAValidEntity()
        {
            var validEntity = new TestClass {Name = "Name"};
            _resultValid = _spec.Evaluate(validEntity);
        }

        protected void WhenTestingAnInValidEntity()
        {
            var validEntity = new TestClass {Name = null};
            _resultInvalid = _spec.Evaluate(validEntity);
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.True(_resultValid == SpecResult.Success);

            Assert.True(_resultInvalid != SpecResult.Success);
        }

        protected void AndThenInvalidSpecResultContainsDetailedErrorMessage()
        {
            Assert.True(_resultInvalid.DetailedErrorMessage == $"Test class 0 must have an id.");
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