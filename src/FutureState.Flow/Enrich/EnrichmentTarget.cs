using FutureState.Flow.Data;
using System.Collections.Generic;
using System.IO;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Gets a target snapshot file or other data source to enrich.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public interface IEnrichmentTarget<TTarget>
    {
        /// <summary>
        ///     Gets the global unique id of the target to enrich. This could be a network address or file system path.
        /// </summary>
        string AddressId { get; }

        /// <summary>
        ///     Gets the entity type being enriched.
        /// </summary>
        FlowEntity FlowEntity { get; }

        /// <summary>
        ///     Gets the underlying entity types to encrich.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TTarget> Get();
    }

    /// <summary>
    ///     Gets the target flow file to enrich.
    /// </summary>
    /// <typeparam name="TSource">The source data type to enrich the target.</typeparam>
    /// <typeparam name="TTarget">The target data type to enrich.</typeparam>
    public class EnrichmentTarget<TSource, TTarget> : IEnrichmentTarget<TTarget>
    {
        private readonly FlowSnapshotRepo<FlowSnapShot<TTarget>> _resultRepo;

        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string AddressId => DataSource.FullName;

        /// <summary>
        ///     Gets the source entity type.
        /// </summary>
        public FlowEntity SourceEntityType => new FlowEntity(typeof(TSource));

        /// <summary>
        ///     Gets the file name.
        /// </summary>
        public FileInfo DataSource { get; set; }

        /// <summary>
        ///     Gets the target flow entity type.
        /// </summary>
        public FlowEntity FlowEntity => new FlowEntity(typeof(TTarget));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnrichmentTarget(FlowSnapshotRepo<FlowSnapShot<TTarget>> repository)
        {
            // targetDirectory repository
            this._resultRepo = repository ?? new FlowSnapshotRepo<FlowSnapShot<TTarget>>();
        }

        /// <summary>
        ///     Gets the process result.
        /// </summary>
        public FlowSnapShot<TTarget> GetProcessResult()
        {
            // load results to get the invalid items
            FlowSnapShot<TTarget> result = _resultRepo.Get(DataSource.FullName);

            return result;
        }

        public IEnumerable<TTarget> Get()
        {
            var results = GetProcessResult();

            // load results to get the invalid items
            foreach (var result in results.Valid)
                yield return result;

            foreach (var result in results.Invalid)
                yield return result;
        }
    }

    public class InMemoryEnrichmentTarget<TTarget> : IEnrichmentTarget<TTarget>
    {
        private readonly string _uniqueAddressId;
        private readonly FlowBatch _batch;
        private readonly IEnumerable<TTarget> _source;

        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string AddressId { get { return _uniqueAddressId; } }

        /// <summary>
        ///     Gets the file name.
        /// </summary>
        public FileInfo DataSource { get; set; }

        /// <summary>
        ///     Gets the target flow entity type.
        /// </summary>
        public FlowEntity FlowEntity => new FlowEntity(typeof(TTarget));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public InMemoryEnrichmentTarget(IEnumerable<TTarget> source, FlowBatch batch, string uniqueAddressId)
        {
            this._uniqueAddressId = uniqueAddressId;
            this._batch = batch;
            this._source = source;
        }

        /// <summary>
        ///     Gets the process result.
        /// </summary>
        public IEnumerable<TTarget> Get()
        {
            // load results to get the invalid items
            return _source;
        }
    }
}