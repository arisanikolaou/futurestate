using CsvHelper;
using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using FutureState.Specifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests.Flow
{
    [Story]
    public class ProcessorsCanBeChainedToFormAnEtlPipeLineStory
    {
        private string _baseDirectory;
        private string _baseDirectoryName;

        private string _dataFileToCreate = @"Test.csv";
        private bool _flowFile1Processed;
        private bool _flowFileProcessed;
        public FlowId _flow;
        private string _inDirectory;
        private string _outDirectory;
        private string _outDirectory2;
        private string _processName;
        private string _processName2;
        private readonly int CsvItemsToCreate = 25;
        private FlowService _flowService;
        private FlowFileLogRepo _logRepository;

        [BddfyFact]
        public void ProcessorsCanBeChainedToFormAnEtlPipeLine()
        {
            this.BDDfy();
        }

        protected void GivenASetOfDirectories()
        {
            _baseDirectoryName = "Flow1";
            _processName = @"MyProcess";

            _baseDirectory = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}";
            if (Directory.Exists(_baseDirectory))
                Directory.Delete(_baseDirectory, true);

            _inDirectory = $@"{_baseDirectory}\{_processName}\In";
            _outDirectory = $@"{_baseDirectory}\{_processName}\Out";
        }

        protected void AndGivenALogRepository()
        {
            _logRepository = new FlowFileLogRepo
            {
                DataDir = _baseDirectory
            };
        }

        protected void AndGivenAFlowService()
        {
            _flowService = new FlowService(new FlowIdRepo());
        }

        protected void AndGivenAGeneratedDataSourceCsvFile()
        {
            if (!Directory.Exists(_inDirectory))
                Directory.CreateDirectory(_inDirectory);

            _dataFileToCreate = $@"{_inDirectory}\{_dataFileToCreate}";

            // always re-create
            if (File.Exists(_dataFileToCreate))
                File.Delete(_dataFileToCreate);

            using (var fs = File.OpenWrite(_dataFileToCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<Enitity1>();

                    csv.Flush();
                    csv.NextRecord();

                    for (var i = 0; i < CsvItemsToCreate; i++)
                    {
                        var entity = new Enitity1
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

        protected void AndGivenAConsistentProcessId()
        {
            _flow = new FlowId("TestFlow1");
        }

        protected void WhenStartingAProcessorService()
        {
            var config = GetConfig<Enitity1, Entity2>();

            config.InDirectory = _inDirectory;
            config.OutDirectory = _outDirectory;

            var batchProcessor = new TestCsvFlowFileFlowFileBatchController(config)
            {
                Flow = _flow
            };

            var processor = new FlowFileControllerService(_flowService, _logRepository, batchProcessor)
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            processor.FlowFileProcessed += (o, e) => { _flowFile1Processed = true; };

            processor.Start();
        }

        private ProcessorConfiguration<TEntityIn, TEntityOut> GetConfig<TEntityIn, TEntityOut>
            () where TEntityOut : class, new()
        {
            var spec = new SpecProvider<TEntityOut>();

            var col = new SpecProvider<IEnumerable<TEntityOut>>();

            return new ProcessorConfiguration<TEntityIn, TEntityOut>(spec, col);
        }

        protected void AndWhenStartingAnotherProcessorService()
        {
            _processName2 = @"MyProcess2";

            _outDirectory2 = $@"{_baseDirectory}\{_processName2}\Out";

            var config =
                GetConfig<Entity2, Entity3>();

            config.InDirectory = _outDirectory;
            config.OutDirectory = _outDirectory2;

            var batchProcessor = new TestProcessResultFlowFileController(config)
            {
                Flow = _flow
            };

            var processor = new FlowFileControllerService(_flowService, _logRepository, batchProcessor)
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            processor.FlowFileProcessed += (o, e) => { _flowFileProcessed = true; };

            processor.Start();
        }

        protected void ThenProcessResultsShouldBeSaved()
        {
            // wait for jobs to finish processing
            var sw = new Stopwatch();
            sw.Start();

            while (!(_flowFile1Processed && _flowFileProcessed) && sw.Elapsed.TotalSeconds < 15)
                Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.True(Directory.GetFiles(_outDirectory).Any());
            Assert.True(Directory.GetFiles(_outDirectory2).Any());
            Assert.True(_flowFileProcessed);
            Assert.True(_flowFile1Processed);
        }

        public class TestCsvFlowFileFlowFileBatchController : CsvFlowFileController<Enitity1, Entity2>
        {
            public TestCsvFlowFileFlowFileBatchController(
                ProcessorConfiguration<Enitity1, Entity2> config) :
                base(config)
            {
            }

            public override Processor<Enitity1, Entity2> GetProcessor()
            {
                var i = 0;
                // preocessor service
                return new Processor<Enitity1, Entity2>(Config)
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

        public class TestProcessResultFlowFileController : FlowSnapshotFileController<Entity2, Entity3>
        {
            public TestProcessResultFlowFileController(ProcessorConfiguration<Entity2, Entity3> config) :
                base(config)
            {
            }

            public override Processor<Entity2, Entity3> GetProcessor()
            {
                var i = 0;
                // preocessor service
                return new Processor<Entity2, Entity3>(Config)
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

        public class Enitity1
        {
            public string Name { get; set; }
        }

        public class Entity2
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }

        public class Entity3
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public DateTime DateProcessed { get; set; }
        }
    }
}