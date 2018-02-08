using System.IO;
using FutureState.Flow.Data;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Gets the target flow file to enrich.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public class EnrichmentTarget<TSource, TTarget>
    {
        /// <summary>
        ///     Gets the unique id of the enrichment target
        /// </summary>
        public string UniqueId { get { return Path.GetFileNameWithoutExtension(File.Name); } }

        /// <summary>
        ///     Gets the file name.
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        ///     Gets the process result.
        /// </summary>
        public ProcessResult<TSource, TTarget> GetProcessResult()
        {
            // targetDirectory repository
            var resultRepo = new ProcessResultRepository<ProcessResult<TSource, TTarget>>();

            // load results to get the invalid items
            ProcessResult<TSource, TTarget> result = resultRepo.Get(File.FullName);

            return result;
        }
    }
}