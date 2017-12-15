using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FutureState.IO
{
    public interface IProcessRunner
    {
        /// <summary>
        /// Execute the given application asynchronously
        /// </summary>
        /// <param name="executable">An executable or batch script to execute</param>
        /// <param name="arguments">Command-line arguments to run with</param>
        /// <param name="environment">Optional environment variables to pass to the applicaton</param>
        Task<ProcessExecutionResult> RunAsync(FileInfo executable,
                                              string[] arguments,
                                              IDictionary<string, string> environment = null);
    }
}
