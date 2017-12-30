using System;
using System.Collections.Generic;
using Xunit;

namespace FutureState.Flow.Tests
{

    public class FlowTests
    {

        [Fact]
        public void CanProcessDataFromSources()
        {
            // test formatting a json file entity source from 
            // one type to another

            var testInput = new List<TestInput>()
            {
                new TestInput()
                {
                    Id = 1,
                    Name = "Name"
                }
            };

            var portSource = new PortSource<TestInput>((i, x, a) =>
            {
                return new ReceiveMessageResponse<TestInput>()
                {
                    Package = new Package<TestInput>()
                    {
                        Data = testInput,
                        CorrelationId = Guid.NewGuid()
                    }
                };
            });

            var subject = new Processor<TestOutput, TestInput>((processor) =>
            {
                var output = new List<TestOutput>();

                foreach (var source in processor.PortSources)
                {
                    var souceInputs = source.Receive("", Guid.Empty, 7);
                    foreach (var entity in souceInputs.Package.Data)
                    {
                        
                    }
                }
                return new ProcessState();
            });

            subject.PortSources.Add(portSource);

            // load processor configuration
            subject.LoadConfiguration();

            // process
            subject.Process();

            // process should notify target
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
