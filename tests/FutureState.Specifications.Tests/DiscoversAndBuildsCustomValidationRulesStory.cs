using System.Collections.Generic;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Specifications.Tests
{
    [Story]
    public class DiscoversAndBuildsCustomValidationRulesStory
    {
        private ISpecification<TestClass>[] _rules;
        private IEnumerable<Error> _errorsTestingCustomRules;
        private IEnumerable<Error> _errorsTestingDefaultAnnotations;

        protected void GivenASpecProviderWithCustomRules()
        {
            var specProvider = new SpecProvider<TestClass>();
            specProvider.Add((s) =>
            {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (s.Name == "John")
                    return new SpecResult("Name can't be John");

                return SpecResult.Success;
            }, "NameNotJohn", "Name can't be john for some reason defined by the business");

            this._rules = specProvider.GetSpecifications().ToArray();
        }

        protected void WhenTestingASubjettWithCustomRule()
        {
            var entity = new TestClass()
            {
                Name = "John"
            };

            // fails custom rule
            this._errorsTestingCustomRules = _rules.ToErrors(entity);
        }

        protected void AndWhenTestingWithDatAnnotations()
        {
            // can't be empty
            this._errorsTestingDefaultAnnotations = _rules.ToErrors(new TestClass());
        }

        protected void ThenAllRulesShouldEvaluateAndBeValid()
        {
            Assert.Equal(2, _rules.Length);

            Assert.Single(_errorsTestingCustomRules);
            Assert.Contains("Name can't be John", _errorsTestingCustomRules.First().Message);

            Assert.Single(_errorsTestingDefaultAnnotations);
            Assert.Contains("can't be empty", _errorsTestingDefaultAnnotations.First().Message);
        }

        [BddfyFact]
        public void DiscoversAndBuildsCustomValidationRules()
        {
            this.BDDfy();
        }

        public class TestClass
        {
            [NotEmpty("Name can't be empty or null.")]
            public string Name { get; set; }
        }
    }
}