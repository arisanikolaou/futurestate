using FutureState.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace FutureState.Flow.Tests
{

    public class FlowTests
    {


        [Fact]
        public void CanProcessDataFromSources()
        {
            var flowId = Guid.Parse("3f97d931-73a9-4ae1-bee1-f1bf00ccd79e");
            var flowDisplayName = "Flow";

            // data source
            var testInput = new List<TestInput>()
            {
                new TestInput()
                {
                    Id = 1,
                    Name = "Name"
                },
                new TestInput()
                {
                    Id = 2,
                    Name = "Name 2"
                }
            };

            // data source provider
            var portSource = new PortSource<TestInput>((sequenceFrom, entitiesCount) =>
            {
                int localIndex = sequenceFrom;
                var outPut = new List<TestInput>();

                for (localIndex = sequenceFrom; localIndex < entitiesCount && localIndex < testInput.Count; localIndex++)
                    outPut.Add(testInput[localIndex]);

                var package = new Package<TestInput>()
                {
                    Data = testInput,
                    Name = flowDisplayName,
                };

                return new QueryResponse<TestInput>(package, localIndex);
            })
            {
                FlowId = flowId
            };

            // processor
            var subject = new Processor<TestOutput, TestInput>((entity) =>
            {
                var outputEntity = new TestOutput()
                {
                    Id = entity.Id,
                    Name = entity.Name + "_transformed",
                    DateCreated = DateTime.UtcNow
                };

                return outputEntity;
            })
            {
                Configuration = new ProcessorConfiguration($"Processor.{typeof(TestOutput).Name}")
            };

            subject.PortSources.Add(portSource);

            // start processing (acti)
            subject.Start();

            // wait to finish processing
            Thread.Sleep(3000);

            var results = subject.Get().ToList();

            Assert.True(results.Count() > 0);
            Assert.Contains(results, m => m.Name.Contains("_transformed"));
        }


        public class TestInput
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }


        public class TestOutput
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public DateTime DateCreated { get; set; }
        }
    }
}
