using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CsvHelper;
using FutureState.Flow.BatchControllers;
using FutureState.Flow.Data;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests.Flow
{
    [Story]
    public class ProcessorsCanBeChainedToFormAnEtlPipeLineStory
    {
        private int CsvItemsToCreate = 25;

        private string _dataFileToCreate = @"Test.csv";
        private string _processName;
        private string _baseDirectory;
        private string _inDirectory;
        private string _outDirectory;
        private string _baseDirectoryName;
        private string _processName2;
        private string _outDirectory2;
        private FlowFileLogRepository _logRepository;
        public Guid _flowId;
        private bool _flowFileProcessed;
        private bool _flowFile1Processed;

        [BddfyFact]
        public void ProcessorsCanBeChainedToFormAnEtlPipeLine()
        {
            this.BDDfy();
        }

        protected void GivenASetOfDirectories()
        {
            this._baseDirectoryName = "Flow";
            this._processName = @"MyProcess";

            this._baseDirectory = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}";
            if(Directory.Exists(_baseDirectory))
                Directory.Delete(_baseDirectory, true);

            this._inDirectory = $@"{_baseDirectory}\{_processName}\In";
            this._outDirectory = $@"{_baseDirectory}\{_processName}\Out";
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

        protected void AndGivenALogRepository()
        {
            this._logRepository = new FlowFileLogRepository()
            {
                WorkingFolder = _baseDirectory
            };
        }

        protected void AndGivenAConsistentProcessId()
        {
            this._flowId = Guid.Parse("b212aeca-130b-4a96-8d30-e3ff4e68c859");
        }


        protected void WhenStartingAProcessorService()
        {
            var batchProcessor = new TestCsvFlowFileFlowFileBatchController()
            {
                InDirectory = _inDirectory,
                OutDirectory = _outDirectory,
                FlowId = _flowId,
            };

            var processor = new FlowFileProcessorService(_logRepository, batchProcessor)
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            processor.FlowFileProcessed += (o, e) =>
            {
                _flowFile1Processed = true;
            };

            processor.Start();
        }

        protected void AndWhenStartingAnotherProcessorService()
        {
            this._processName2 = @"MyProcess2";

            this._outDirectory2 = $@"{_baseDirectory}\{_processName2}\Out";


            var batchProcessor = new TestProcessResultFlowFileBatchController()
            {
                InDirectory = _outDirectory,
                OutDirectory = _outDirectory2,
                FlowId = _flowId,
            };

            var processor = new FlowFileProcessorService(_logRepository, batchProcessor)
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            processor.FlowFileProcessed += (o, e) =>
            {
                _flowFileProcessed = true;
            };

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

        public class TestCsvFlowFileFlowFileBatchController : CsvFlowFileFlowFileBatchController<Enitity1, Entity2>
        {
            public override Processor<Enitity1, Entity2> GetProcessor()
            {
                int i = 0;
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

        public class TestProcessResultFlowFileBatchController : ProcessResultFlowFileBatchController<Entity2, Entity3>
        {
            public override Processor<Entity2, Entity3> GetProcessor()
            {
                int i = 0;
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
