using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using CsvHelper;
using FutureState.Flow.BatchControllers;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests.Flow
{
    [Story]
    public class ProcessorsCanBeChainedToFormAnEtlPipeLineWithAutofacStory
    {
        private int CsvItemsToCreate = 25;

        private readonly string DataFileName = @"TestData.csv";

        private IContainer _container;
        private FlowConfiguration _flowConfig;
        private FlowController _flowController;

        [BddfyFact]
        public void ProcessorsCanBeChainedToFormAnEtlPipeLineWithAutofac()
        {
            this.BDDfy();
        }

        protected void GivenAnInitializedFlowConfig()
        {
            var baseDirectoryName = "Flow";

            var baseDirectory = $@"{Environment.CurrentDirectory}\{baseDirectoryName}";
            if (Directory.Exists(baseDirectory))
                Directory.Delete(baseDirectory, true);

            var flowConfig = new FlowConfiguration(Guid.Parse("b212aeca-130b-4a96-8d30-e3ff4e68c860"))
            {
                BasePath = baseDirectory,
            };

            flowConfig.AddController<TestCsvFlowFileFlowFileBatchController>("ProcessorA");
            flowConfig.AddController<TestProcessResultFlowFileBatchController>("ProcessorB");

            this._flowConfig = flowConfig;
        }

        protected void AndGivenAGeneratedDataSourceCsvFile()
        {
            var baseDir = this._flowConfig.Controllers.First().InputDirectory;
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            string csvFilePath = $@"{baseDir}\{DataFileName}";

            // always re-create
            if (File.Exists(csvFilePath))
                File.Delete(csvFilePath);

            using (var fs = File.OpenWrite(csvFilePath))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<EnitityA>();

                    csv.Flush();
                    csv.NextRecord();


                    for (var i = 0; i < CsvItemsToCreate; i++)
                    {
                        var entity = new EnitityA
                        {
                            Name = $"Key-{i}"
                        };

                        csv.WriteRecord(entity);
                        csv.NextRecord();
                    }

                    csv.Flush();
                }
            }
        }

        protected void AndGivenAnInitializedDiContainer()
        {
            var cb = new ContainerBuilder();

            cb.RegisterModule(new FlowModule());

            // register controllers
            cb.RegisterType<TestCsvFlowFileFlowFileBatchController>().AsSelf().AsImplementedInterfaces();
            cb.RegisterType<TestProcessResultFlowFileBatchController>().AsSelf().AsImplementedInterfaces();

            _container = cb.Build();
        }

        protected void AndGivenAFlowControllerUsingThisConfiguration()
        {
            _flowController = _container.Resolve<FlowController>();
        }


        protected void WhenRunningTheFlowController()
        {
            _flowController.Start(_flowConfig);
        }

        protected void AndWhenSavingTheFlowConfig()
        {
            _flowConfig.Save();
        }

        protected void ThenProcessedResultsShouldBeSaved()
        {
            // wait for jobs to finish processing
            var sw = new Stopwatch();
            sw.Start();
            while (_flowController.Processed <= 2 && sw.Elapsed.TotalSeconds < 15)
                Thread.Sleep(TimeSpan.FromSeconds(1));

            foreach (FlowControllerDefinition flowControllerDefinition in _flowConfig.Controllers)
                Assert.True(Directory.GetFiles(flowControllerDefinition.OutputDirectory).Length == 1);

            Assert.True(_flowController.Processed == 2);
        }

        protected void AndThenShouldBeAbleToRepeatProcessingFromConfiguration()
        {
            var config = FlowConfiguration.Load(@"Flow\flow-config.yaml");
            var controller = _container.Resolve<FlowController>();

            // clear prior results
            Directory.Delete(_flowConfig.BasePath, true);

            // re-create csv
            AndGivenAGeneratedDataSourceCsvFile();

            // start
            controller.Start(config);

            // wait for jobs to finish processing
            var sw = new Stopwatch();
            sw.Start();
            while (controller.Processed <= 2 && sw.Elapsed.TotalSeconds < 15)
                Thread.Sleep(TimeSpan.FromSeconds(1));

            // resave
            _flowConfig.Save();

            foreach (FlowControllerDefinition flowControllerDefinition in _flowConfig.Controllers)
                Assert.True(Directory.GetFiles(flowControllerDefinition.OutputDirectory).Length == 1);
        }

        public class TestCsvFlowFileFlowFileBatchController : CsvFlowFileFlowFileBatchController<EnitityA, EntityB>
        {
            public override Processor<EnitityA, EntityB> GetProcessor()
            {
                int i = 0;
                // preocessor service
                return new Processor<EnitityA, EntityB>(Config)
                {
                    BeginProcessingItem = (dtoIn, dtoOut) =>
                    {
                        if (_logger.IsInfoEnabled)
                            _logger.Info($"Processing records: {JsonConvert.SerializeObject(dtoOut)}");

                        dtoOut.Name = dtoIn.Name;
                        dtoOut.Id = ++i;
                    }
                };
            }
        }

        public class TestProcessResultFlowFileBatchController : ProcessResultFlowFileBatchController<EntityB, EntityC>
        {
            public override Processor<EntityB, EntityC> GetProcessor()
            {
                int i = 0;
                // preocessor service
                return new Processor<EntityB, EntityC>(Config)
                {
                    BeginProcessingItem = (dtoIn, dtoOut) =>
                    {
                        if (_logger.IsInfoEnabled)
                            _logger.Info($"Processing records: {JsonConvert.SerializeObject(dtoOut)}");

                        dtoOut.Name = dtoIn.Name;
                        dtoOut.Id = ++i + dtoIn.Id;
                        dtoOut.DateProcessed = DateTime.UtcNow;
                    }
                };
            }
        }

        public class EnitityA
        {
            public string Name { get; set; }
        }

        public class EntityB
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }

        public class EntityC
        {
            public string Name { get; set; }

            public int Id { get; set; }


            public DateTime DateProcessed { get; set; }
        }
    }
}
