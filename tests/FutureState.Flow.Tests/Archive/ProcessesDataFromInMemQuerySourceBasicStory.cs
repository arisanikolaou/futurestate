using FutureState.Data;
using FutureState.Flow.QuerySources;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace FutureState.Flow.Tests
{
    [Story]
    public class ProcessesDataFromInMemQuerySourceBasicStory : IDisposable
    {
        private Guid flowId;
        private string flowDisplayName;
        private List<TestInput> testInput;
        private readonly ITestOutputHelper output;
        private QuerySource<TestInput> portSource;
        private Processor<TestOutput, TestInput> subject;
        private List<TestOutput> results;
        private List<ProcessEntityError> invalidData;

        public void GivenAWellKnownFlow()
        {
            this.flowId = Guid.Parse("3f97d931-73a9-4ae1-bee1-f1bf00ccd79e");
            this.flowDisplayName = "Flow";
        }

        public void AndGivenAnInMemoryDataSource()
        {
            // data source
            this.testInput = new List<TestInput>();
            for (int i = 0; i < 3; i++)
                testInput.Add(new TestInput()
                {
                    Id = i,
                    Name = "Name:" + i
                });
        }

        public void AndGivenAValidQuerySourceForThisData()
        {
            // data source provider
            this.portSource = new QuerySourceInMemory<TestInput>(flowId, testInput); ;
        }


        public void AndGivenAProcessor()
        {
            // processor
            int localIndexCount = 0;

            var config = new ProcessorConfiguration($"Processor.{typeof(TestOutput).Name}")
            {
                FlowDirPath = Environment.CurrentDirectory
            };

            //this.subject = new Processor<TestOutput, TestInput>((inputEntity) =>
            //{
            //    localIndexCount++;

            //    // map incoming data to outgoing data
            //    var outputEntity = new TestOutput()
            //    {
            //        Id = inputEntity.Id,
            //        Name = inputEntity.Name + "_transformed" + localIndexCount,
            //        DateCreated = DateTime.UtcNow,
            //        Description = localIndexCount % 2 == 0 ? null : "Description" + localIndexCount
            //    };

            //    return outputEntity;
            //},
            //config, 
            //new SpecProvider<TestOutput>(),
            //new SpecProvider<IEnumerable<TestOutput>>(), 
            //portSource);
        }

        public void WhenStartingProcessing()
        {
            // start processing (active)
            //subject.Start();

            // wait to finish processing and for time to elapse
            Thread.Sleep(3000);
        }

        public void AndWhenSubsequentlyQueryingTheDataProcessed()
        {
            //this.results = subject.GetValidData().ToList();
            //this.invalidData = subject.GetInvalidData().ToList();
        }

        public void ThenResultsProcessedShouldBePersistedBothValidAndInvalid()
        {
            // assert that the results were transformed
            Assert.True(results.Any());

            // should contain some invalid data (based on data annotations)
            output.WriteLine("Validation rules:");
            Assert.True(invalidData.Any());
            foreach (var error in invalidData)
                foreach (var e in error.Errors)
                    output.WriteLine(e.Message);

            Assert.Contains(results, m => m.Name.Contains("_transformed"));
        }

        public ProcessesDataFromInMemQuerySourceBasicStory(ITestOutputHelper output)
        {
            this.output = output;
        }

        [BddfyFact]
        public void ProcessesDataFromInMemQuerySourceBasic()
        {
            this.BDDfy();
        }

        public void Dispose()
        {
            //this.subject?.Dispose();
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

            [Required]
            public string Description { get; set; }
        }
    }
}
