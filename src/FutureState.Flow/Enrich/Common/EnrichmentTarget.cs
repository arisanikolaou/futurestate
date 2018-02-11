using System.IO;
using FutureState.Flow.Data;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    public interface IEnrichmentTarget<TTarget>
    {
        /// <summary>
        ///     Gets the global unique id of the target to enrich. This could be a network address or file system path.
        /// </summary>
        string AddressId { get;}

        /// <summary>
        ///     Gets the underlying entity types to encrich.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TTarget> Get();

        /// <summary>
        ///     Gets the batch process that was used to generate the target if any.
        /// </summary>
        /// <returns></returns>
        FlowBatch GetBatch();
    }

    /// <summary>
    ///     Gets the target flow file to enrich.
    /// </summary>
    /// <typeparam name="TSource">The source data type to enrich the target.</typeparam>
    /// <typeparam name="TTarget">The target data type to enrich.</typeparam>
    public class EnrichmentTarget<TSource, TTarget> : IEnrichmentTarget<TTarget>
    {
        private readonly ProcessResultRepository<ProcessResult<TSource, TTarget>> _resultRepo;

        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string AddressId
        {
            get { return DataSource.FullName; }
        }

        public FlowEntity SourceEntityType
        {
            get
            {
                return new FlowEntity(typeof(TSource));
            }
        }

        /// <summary>
        ///     Gets the file name/
        /// </summary>
        public FileInfo DataSource { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnrichmentTarget(ProcessResultRepository<ProcessResult<TSource, TTarget>> repository)
        {
            // targetDirectory repository
            this._resultRepo = repository ?? new ProcessResultRepository<ProcessResult<TSource, TTarget>>();
        }

        /// <summary>
        ///     Gets the process result.
        /// </summary>
        public ProcessResult<TSource, TTarget> GetProcessResult()
        {
            // load results to get the invalid items
            ProcessResult<TSource, TTarget> result = _resultRepo.Get(DataSource.FullName);

            return result;
        }

        public IEnumerable<TTarget> Get()
        {
            var results = GetProcessResult();

            // load results to get the invalid items
            foreach (var result in results.Output)
                yield return result;

            foreach (var result in results.Invalid)
                yield return result;
        }

        public FlowBatch GetBatch()
        {
            return new FlowBatch() { Flow = new Flow("Test"), BatchId = 1 };
        }
    }

    public class InMemoryEnrichmentTarget<TTarget> : IEnrichmentTarget<TTarget>
    {
        readonly string _uniqueAddressId;
        private readonly FlowBatch _batch;
        private readonly IEnumerable<TTarget> _source;

        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string AddressId { get { return _uniqueAddressId; } }

        /// <summary>
        ///     Gets the file name/
        /// </summary>
        public FileInfo DataSource { get; set; }

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

        public FlowBatch GetBatch()
        {
            return _batch;
        }
    }
}