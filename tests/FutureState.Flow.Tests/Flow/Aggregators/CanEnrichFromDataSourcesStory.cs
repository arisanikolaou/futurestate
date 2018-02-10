﻿using System;
using System.Collections.Generic;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Enrich
{
    [Story]
    public class CanEnrichFromDataSourcesStory
    {
        private string _sourceId = "Source";
        private List<Whole> _source;
        private List<IEnricher<Whole>> _enrichers;
        private EnrichmentLog _processResults;
        private BatchProcess _process;
        private EnricherProcessor _controller;
        private EnrichmentLog _loadedResults;

        protected void GivenAnInMemorySetOfWholeAndPartSources()
        {
            _source = new List<Whole>()
            {
                new Whole() { Key = "Key1" },
                new Whole() { Key = "Key2" }
            };

            var parts = new List<Part>()
            {
                new Part() {Key = "Key1", FirstName = "FirstName"},
                new Part() {Key = "Key2", FirstName = "FirstName2"}
            };

            var parts2 = new List<OtherPart>()
            {
                new OtherPart() {Key = "Key1", LastName = "LastName"}
            };

            var enricher = new Enricher<Part, Whole>(() => parts) {OutputTypeId = "EnricherA"};
            var enricher1 = new Enricher<OtherPart, Whole>(() => parts2) { OutputTypeId = "EnricherB" };

            // collect all enrichment sources
            _enrichers = new List<IEnricher<Whole>> { enricher, enricher1 };
        }

        protected void AndGivenABatchProcess()
        {
            this._process = new BatchProcess(
                Guid.Parse("f41cfe3a-4ddb-43ae-8302-0c322c84bdd1"),
                1);
        }

        protected void AndGivenAnEnrichingController()
        {
            this._controller = new EnricherProcessor();
        }

        protected void WhenProcessingEnrichmentsAgainstTheSource()
        {
            _processResults = _controller.Enrich( _source, _enrichers);
        }

        protected void AndWhenSavingResults()
        {
            var repo = new EnricherLogRepository();

            repo.Save(this._processResults);
        }

        protected void ThenAllEligibleWholeItemsShouldBeMerged()
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

            // first enricher should process at least two entities
            Assert.Equal(
                _enrichers.Count, 
                _processResults.Logs
                    .FirstOrDefault( m => m.OutputTypeId == "EnricherA")
                    ?.EntitiesEnriched);
        }

        protected void AndThenShouldBeAbleToSaveEnricherResultsResults()
        {
            var repo = new EnricherLogRepository();

            // should be able to reload
            this._loadedResults = repo.Get("Whole");

            Assert.NotNull(_loadedResults);
            Assert.Equal(2, _loadedResults.Logs.Count);

            foreach (var enricher in _enrichers)
            {
                Assert.True(
                    _loadedResults.GetHasBeenProcessed( enricher));
            }
        }

        [BddfyFact]
        public void CanEnrichFromDataSources()
        {
            this.BDDfy();
        }
    }
}