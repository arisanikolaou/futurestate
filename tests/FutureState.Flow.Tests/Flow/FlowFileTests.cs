using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CsvHelper;
using FutureState.Flow.Core;
using FutureState.Flow.Flow;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests.Flow
{
    [Story]
    public class ReadsFlowFilesAsAServiceStory
    {
        private int CsvItemsToCreate = 25;

        private string _dataFileToCreate = @"Test.csv";
        private string _processName;
        private string _baseDirectory;
        private string _inDirectory;
        private string _outDirectory;
        private ProcessResultRepository<ProcessResult<TSourceEntity, TargetEntity2>> _repository;
        private string _baseDirectoryName;
        private string _processName2;
        private string _outDirectory2;
        private FlowFileLogRepository _logRepository;

        [BddfyFact]
        public void Tests()
        {
            this.BDDfy();
        }

        protected void GivenASetOfDirectories()
        {
            this._baseDirectoryName = "Flow";
            this._processName = @"MyProcess";

            this._baseDirectory = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}\{_processName}";
            this._inDirectory = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}\{_processName}\In";
            this._outDirectory = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}\{_processName}\Out";
        }

        protected void AndGivenAProcessResultRepo()
        {
            _repository = new ProcessResultRepository<ProcessResult<TSourceEntity, TargetEntity2>>(_outDirectory);
        }

        protected void AndGivenAGeneratedDataSourceCsvFile()
        {
            if (!Directory.Exists(_inDirectory))
                Directory.CreateDirectory(_inDirectory);

            _dataFileToCreate = $@"{_inDirectory}\DataFileToCreate";

            // always re-create
            if (File.Exists(_dataFileToCreate))
                File.Delete(_dataFileToCreate);

            using (var fs = File.OpenWrite(_dataFileToCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<TSourceEntity>();

                    csv.Flush();
                    csv.NextRecord();


                    for (var i = 0; i < CsvItemsToCreate; i++)
                    {
                        var entity = new TSourceEntity
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
            this.ProcessId = SeqGuid.Create();
        }

        public Guid ProcessId { get; set; }


        protected void WhenStartingAProcessorService()
        {
            var batchProcessor = new CsvFlowFileBatchProcessor2()
            {
                InDirectory = _inDirectory,
                OutDirectory = _outDirectory
            };

            var processor = new FlowFileProcessor(_logRepository, batchProcessor)
            {
                ProcessId = ProcessId,
                Interval = TimeSpan.FromSeconds(5)
            };

            processor.Start();
        }

        protected void AndWhenStartingAnotherProcessorService()
        {
            this._processName2 = @"MyProcess2";

            this._outDirectory2 = $@"{Environment.CurrentDirectory}\{_baseDirectoryName}\{_processName2}\Out";


            var batchProcessor = new ProcessResultBatchProcessor2()
            {
                InDirectory = _outDirectory,
                OutDirectory = _outDirectory2
            };

            var processor = new FlowFileProcessor(_logRepository, batchProcessor)
            {
                ProcessId = ProcessId,
                Interval = TimeSpan.FromSeconds(5)
            };

            processor.Start();
        }

        protected void ThenProcessResultsShouldBeSaved()
        {
            Thread.Sleep(TimeSpan.FromMinutes(5));

            Assert.True(Directory.GetFiles(_outDirectory).Any());
            Assert.True(Directory.GetFiles(_outDirectory2).Any());
        }

        public class CsvFlowFileBatchProcessor2 : CsvFlowFileBatchProcessor<TSourceEntity, TargetEntity2>
        {
            public override Processor<TSourceEntity, TargetEntity2> Configure()
            {
                var config = new ProcessorConfiguration<TSourceEntity, TargetEntity2>();

                // where results will be posted to
                // create engine to save results to out directory
                var engine = new ProcessorEngine<TSourceEntity>(Name);

                int i = 0;
                // preocessor service
                return new Processor<TSourceEntity, TargetEntity2>(config, engine)
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

        public class ProcessResultBatchProcessor2 : ProcessResultBatchProcessor<TargetEntity2, TargetEntity3>
        {
            public override Processor<TargetEntity2, TargetEntity3> Configure()
            {
                var config = new ProcessorConfiguration<TargetEntity2, TargetEntity3>();

                // where results will be posted to
                // create engine to save results to out directory
                var engine = new ProcessorEngine<TargetEntity2>(Name);

                int i = 0;
                // preocessor service
                return new Processor<TargetEntity2, TargetEntity3>(config, engine)
                {
                    BeginProcessingItem = (dtoIn, dtoOut) =>
                    {
                        if (_logger.IsInfoEnabled)
                            _logger.Info($"Processing records: {JsonConvert.SerializeObject(dtoOut)}");

                        dtoOut.Name = dtoIn.Name;
                        dtoOut.Id = ++i + dtoIn.Id;
                    }
                };
            }
        }

        public abstract class CsvFlowFileBatchProcessor<TIn, TOut> : FlowFileBatchProcessor<TIn, TOut> 
            where TOut : class, new()
        {
            protected CsvFlowFileBatchProcessor() : base(new CsvProcessorReader<TIn>())
            {

            }
        }

        public abstract class ProcessResultBatchProcessor<TIn, TOut> : FlowFileBatchProcessor<TIn, TOut>
            where TOut : class, new()
        {
            public ProcessResultBatchProcessor() : base(GetReader())
            {


            }

            static IReader<TIn> GetReader()
            {
                return new GenericResultReader<TIn>((dataSource) =>
                {
                    var repoository = new ProcessResultRepository<ProcessResult<TOut, TIn>>(dataSource);

                    var processResult = repoository.Get(dataSource);

                    return processResult.Output;
                });
            }
        }

        public class TSourceEntity
        {
            public string Name { get; set; }
        }

        public class TargetEntity2
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }

        public class TargetEntity3
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }
    }
}
