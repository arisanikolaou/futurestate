using Xunit;
using Xunit.Abstractions;

namespace FutureState.Common.Tests
{
    public class ErrorExtTests
    {
        private readonly ITestOutputHelper _output;

        public ErrorExtTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ThrowsErrorsIfEnumerableIsPopulated()
        {
            var errors = new[]
            {
                new Error("Message", "Type"),
                new Error("Message-2", "Type"),
            };

            Assert.Throws<RuleException>(() => { errors.ThrowIfExists("One or more errors were detected."); });
        }

        [Fact]
        public void DoesNotThrowErrorIfEnumerableIsPopulated()
        {
            var errors = new Error[] { };

            errors.ThrowIfExists(); //should not throw error
        }

        [Fact]
        public void ProducesListString()
        {
            var errors = new[]
            {
                new Error("Message", "Type"),
                new Error("Message-2", "Type"),
            };

            string output = errors.ToListString();

            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.Contains("Message-2", output);
            Assert.Contains("Message", output);

            // produce output string
            _output.WriteLine(output);
        }
    }
}