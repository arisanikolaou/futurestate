using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using CsvHelper;
using FutureState.Flow.Controllers;
using FutureState.Specifications;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;
using IContainer = Autofac.IContainer;

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
            var baseDirectoryName = "Flow2";

            var baseDirectory = $@"{Environment.CurrentDirectory}\{baseDirectoryName}";
            if (Directory.Exists(baseDirectory))
                Directory.Delete(baseDirectory, true);

            var flowConfig = new FlowConfiguration(Guid.Parse("b212aeca-130b-4a96-8d30-e3ff4e68c860"))
            {
                BasePath = baseDirectory,
            };

            var def1 = flowConfig.AddController<TestCsvFlowController>("ProcessorA");
            def1.FieldValidationRules.Add(new ValidationRule()
            {
                FieldName = "Value",
                ErrorMessage = "Value must be numeric",
                RegEx = "^[0-9]*$"
            });

            // configuration path
            def1.ConfigurationDetails.Add("ValueToConfigure", "http://helplnk.etc");

            flowConfig.AddController<TestProcessResultFlowFileBatchController>("ProcessorB");

            this._flowConfig = flowConfig;
        }

        protected void AndGivenAGeneratedDataSourceCsvFile()
        {
            var baseDir = this._flowConfig.Controllers.First().Input;
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
            cb.RegisterType<TestCsvFlowController>().AsSelf().AsImplementedInterfaces();
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

        protected void AndWhenSavingTheFlowConfigAndUsingControllerFriendlyNames()
        {
            // ReSharper disable once PossibleNullReferenceException
            var lastController = _flowConfig.Controllers.LastOrDefault();
            Assert.NotNull(lastController);

            lastController
                .ConfigurationDetails.Add("Item1", "Value1");
            lastController.TypeName = "";

            // should match the display name of the controller type
            lastController.ControllerName = "ProcessorB";

            _flowConfig.Save();
        }

        protected void ThenProcessedResultsShouldBeSaved()
        {
            // wait for jobs to finish processing
            var sw = new Stopwatch();
            sw.Start();

            // spin wait for background work to finish
            while (_flowController.Processed <= 20 && sw.Elapsed.TotalSeconds < 15)
                Thread.Sleep(TimeSpan.FromSeconds(1));

            foreach (FlowControllerDefinition flowControllerDefinition in _flowConfig.Controllers)
                Assert.True(Directory.GetFiles(flowControllerDefinition.Output).Length == 1);

            Assert.True(_flowController.Processed == 2);
        }

        protected void ThenConfigurationSystemShouldBuildValidators()
        {
            var specs = _container
                .Resolve<IProvideSpecifications<EntityB>>()
                .GetSpecifications().ToArray();

            // regex validator should be set
            Assert.Single(specs);
        }

        protected void AndThenFlowCustomConfigurationShouldBeSet()
        {
            // flow conrtroller should configure all controllers
            IFlowFileController[] controllers = _flowController.GetControllers();
            foreach(var controller in controllers)
            {
                if(controller is TestCsvFlowController)
                {
                    var testCsv = controller as TestCsvFlowController;
                    Assert.Equal("http://helplnk.etc", testCsv.ValueToConfigure);
                }
            }
        }

        protected void AndThenShouldBeAbleToRepeatProcessingFromConfiguration()
        {
            var config = FlowConfiguration.Load($@"{_flowConfig.BasePath}\flow-config.yaml");
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

            // spin wait
            while (controller.Processed <= 2 && sw.Elapsed.TotalSeconds < 15)
                Thread.Sleep(TimeSpan.FromSeconds(1));

            // resave
            _flowConfig.Save();

            foreach (FlowControllerDefinition flowControllerDefinition in _flowConfig.Controllers)
                Assert.True(Directory.GetFiles(flowControllerDefinition.Output).Length == 1);
        }

        public class TestCsvFlowController : CsvFlowFileController<EnitityA, EntityB>
        {
            public string ValueToConfigure { get; set; }

            public TestCsvFlowController(ProcessorConfiguration<EnitityA, EntityB> config)
                : base(config)
            {

            }

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

                        if(i % 2 == 0)
                            dtoOut.Value = "A"; // should not be valid
                        else
                            dtoOut.Value = "123"; // should not be valid
                    }
                };
            }
        }

        // used in file configuration
        [DisplayName("ProcessorB")]
        public class TestProcessResultFlowFileBatchController : ProcessResultFlowFileBatchController<EntityB, EntityC>
        {
            public TestProcessResultFlowFileBatchController(ProcessorConfiguration<EntityB, EntityC> config)
       : base(config)
            {

            }

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

            public string Value { get; set; }
        }

        public class EntityC
        {
            public string Name { get; set; }

            public int Id { get; set; }


            public DateTime DateProcessed { get; set; }
        }
    }
}
