﻿using Xunit;
using Xunit.Abstractions;

namespace FutureState.Common.Tests
{
    public class ErrorExtTests
    {
        public ErrorExtTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

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
                new Error("Message-2", "Type")
            };

            var output = errors.ToListString();

            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.Contains("Message-2", output);
            Assert.Contains("Message", output);

            // produce output string
            _output.WriteLine(output);
        }

        [Fact]
        public void ThrowsErrorsIfEnumerableIsPopulated()
        {
            var errors = new[]
            {
                new Error("Message", "Type"),
                new Error("Message-2", "Type")
            };


            Assert.Throws<RuleException>(() => { errors.ThrowIfExists("One or more errors were detected."); });
        }
    }
}