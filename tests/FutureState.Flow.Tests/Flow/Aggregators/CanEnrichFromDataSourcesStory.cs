using System;
using System.Collections.Generic;
using System.IO;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Enrich
{
    [Story]
    public class CanEnrichFromDataSourcesStory
    {
        private List<Whole> _source;
        private InMemoryEnrichmentTarget<Whole> _enrichmentTarget;
        private List<IEnricher<Whole>> _enrichers;
        private EnricherLogRepository _repo;
        private EnricherProcessor _controller;
        private FlowId _flow;
        private FlowBatch _flowBatch;
        private FlowService _flowService;

        protected void GivenAnInMemorySetOfWholeAndPartSources()
        {
            // one source - one log
            var parts = new List<Part>()
            {
                new Part() {Key = "Key1", FirstName = "FirstName"},
                new Part() {Key = "Key2", FirstName = "FirstName2"}
            };

            // second source - another log
            var otherPart = new List<OtherPart>()
            {
                new OtherPart() {Key = "Key1", LastName = "LastName"}
            };

            var enricher = new Enricher<Part, Whole>(() => parts, "Part");
            var otherEnricher = new Enricher<OtherPart, Whole>(() => otherPart, "OtherPart");

            // collect all enrichment sources
            _enrichers = new List<IEnricher<Whole>> { enricher, otherEnricher };
        }

        protected void AndGivenANewFlow()
        {
            // cleanup any existing file
            foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "*Test*.json"))
                File.Delete(file);

            this._flowService = new FlowService(new FlowIdRepo());

            this._flow = this._flowService.CreateNew("Test");
            this._flowBatch = this._flowService.GetNewFlowBatch("Test");
        }

        protected void AndGivenAnEnrichmentTarget()
        {
            _source = new List<Whole>()
            {
                new Whole() { Key = "Key1" },
                new Whole() { Key = "Key2" }
            };

            this._enrichmentTarget = new InMemoryEnrichmentTarget<Whole>(_source, _flowBatch, "UniqueTargetId");
        }

        protected void AndGivenAnEnrichingController()
        {
            this._repo = new EnricherLogRepository();
            this._controller = new EnricherProcessor(_repo);
        }

        protected void WhenProcessingEnrichmentsAgainstTheSource()
        {
            _controller.Enrich(_flowBatch, new[] { _enrichmentTarget }, _enrichers);
        }

        protected void ThenAllEligibleWholeItemsShouldBeEnrichedFromSource()
        {
            foreach (var whole in _source)
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (whole.Key == "Key1")
                {
                    Assert.True(whole.FirstName != null);
                    Assert.True(whole.LastName != null);
                }
                else if (whole.Key == "Key2")
                {
                    Assert.True(whole.FirstName != null);
                    Assert.True(whole.LastName == null);
                }
            }
        }

        protected void AndThenAnEnrichmentLogShouldBeSavedForAllSources()
        {
            {
                // should be able to reload
                var db = _repo.Get(_flow, new FlowEntity(typeof(Part)));

                Assert.NotNull(db);
                Assert.Single(db.Logs);
            }

            {
                // should be able to reload
                var db = _repo.Get(_flow, new FlowEntity(typeof(OtherPart)));

                Assert.NotNull(db);
                Assert.Single(db.Logs);
            }
        }

        [BddfyFact]
        public void CanEnrichFromDataSources()
        {
            this.BDDfy();
        }
    }
}