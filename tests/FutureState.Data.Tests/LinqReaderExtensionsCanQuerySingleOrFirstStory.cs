using System;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Data.Tests
{
    using System.ComponentModel.DataAnnotations;

    [Story]
    public class LinqReaderExtensionsCanQuerySingleOrFirstStory
    {
        private ILinqReader<TestEntity, Guid> _reader;
        private TestEntity _first;
        private TestEntity _single;

        public void GivenALinqReaderWithData()
        {
            var repo = new InMemoryRepository<TestEntity, Guid>();
            _reader = repo;

            // insert
            repo.Insert(new TestEntity()
            {
                Id = Guid.NewGuid(),
                Name = "Name"
            });

            repo.Insert(new TestEntity()
            {
                Id = Guid.NewGuid(),
                Name = "Name 2"
            });
        }

        public void WhenQueryingFirst()
        {
            _first = _reader.First(m => m.Name == "Name");
        }

        public void WhenQueringSingle()
        {
            _single = _reader.Single(m => m.Name == "Name");
        }


        public void ThenFirstAndSingleShouldReturnValidResults()
        {
            Assert.NotNull(_first);
            Assert.NotNull(_single);
        }

        public void ThenShouldThrowInvalidOperationExceptionWhenNotResultsFound()
        {
            Assert.Throws<InvalidOperationException>(() => _reader.First(m => m.Name == null));
            Assert.Throws<InvalidOperationException>(() => _reader.Single(m => m.Name == null));
        }

        [BddfyFact]
        public void LinqReaderExtensionsCanQuerySingleOrFirst()
        {
            this.BDDfy();
        }

        public class TestEntity : IEntity<Guid>
        {
            [Key]
            public Guid Id { get; set; }

            public string Name { get; set; }
        }
    }
}
