using System.IO;
using FutureState.Flow.Data;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Gets the target flow file to enrich.
    /// </summary>
    /// <typeparam name="TSource">The source data type to enrich the target.</typeparam>
    /// <typeparam name="TTarget">The target data type to enrich.</typeparam>
    public class EnrichmentTarget<TSource, TTarget>
    {
        private readonly ProcessResultRepository<ProcessResult<TSource, TTarget>> _resultRepo;

        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string UniqueId { get { return Path.GetFileNameWithoutExtension(SourceFileName.Name); } }

        /// <summary>
        ///     Gets the file name/
        /// </summary>
        public FileInfo SourceFileName { get; set; }

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
            ProcessResult<TSource, TTarget> result = _resultRepo.Get(SourceFileName.FullName);

            return result;
        }
    }
}