using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Specifications.Tests
{
    [Story]
    public class CanValidateEmailDeclaritivelyStory
    {
        private ISpecification<TestEntity>[] _entitySpecs;
        private Error[] _errorsFromInvalidEntities;
        private Error[] _errorsFromValidEntity;

        protected void GivenASpecProvider()
        {
            var specProvider = new SpecProvider<TestEntity>();

            this._entitySpecs = specProvider.GetSpecifications().ToArray();
        }

        protected void WhenValidatingAEntityWithAnInValidEmailAttribute()
        {
            var invalidEntity = new TestEntity()
            {
                Email = "invalidentity"
            };

            // errors
            this._errorsFromInvalidEntities = _entitySpecs.ToErrors(invalidEntity).ToArray();
       }

        protected void WhenValidatingAEntityWithAnValidEmailAttribute()
        {
            var validEntity = new TestEntity()
            {
                Email = "name@address.com"
            };

            // errors
            this._errorsFromValidEntity = _entitySpecs.ToErrors(validEntity).ToArray();
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.True(_errorsFromInvalidEntities.Length > 0);
            Assert.Contains(_errorsFromInvalidEntities, m => m.Message.Contains("'Email' does not meet the required pattern or range."));

            Assert.True(_errorsFromValidEntity.Length == 0);
        }

        [BddfyFact]
        public void CanValidateEmailDeclaritively()
        {
            this.BDDfy();
        }

        public class TestEntity
        {
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}
