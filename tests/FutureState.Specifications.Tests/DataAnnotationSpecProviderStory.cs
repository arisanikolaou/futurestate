using FluentAssertions;
using FutureState;
using FutureState.Specifications;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Specifications.Tests
{
    [Collection("DataAnnotationSpecProviderStory")]
    public class DataAnnotationSpecProviderStory
    {
        private DataAnnotationsSpecProvider<TestEntity> _subject;
        private TestEntity _invalidEntity;
        private Error[] _errorsOnValidEntity;

        void GivenADataAnotationProvider()
        {
            _subject = new DataAnnotationsSpecProvider<TestEntity>();
        }

        void WhenValidatingAnInvalidEntity()
        {
            _invalidEntity = new TestEntity();

            _errorsOnValidEntity = _subject.GetSpecifications().ToErrors(_invalidEntity).ToArray(); 
        }

        void AndWhenValidatingAValidEntity()
        {
            _invalidEntity = new TestEntity() { Name = "Name" };

            _errorsOnValidEntity = _subject.GetSpecifications().ToErrors(_invalidEntity).ToArray();
        }


        void ShouldReportErrorsAsAppropriate()
        {
            Assert.Single(_errorsOnValidEntity);
            Assert.Empty(_errorsOnValidEntity);
        }

        [BddfyFact]
        public void DataAnnotationSpecProvider()
        {
            this.BDDfy();
        }

        public class TestEntity
        {
            [Required]
            public string Name { get; set; }
        }
    }
}
