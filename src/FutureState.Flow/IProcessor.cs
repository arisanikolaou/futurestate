using System.Collections.Generic;
using System.IO;
using FutureState.Flow.Flow;

namespace FutureState.Flow
{
    /// <summary>
    ///     Process data from a given incoming source.
    /// </summary>
    public interface IProcessor
    {
        string ProcessName { get; }
    }
    
    public interface IBatchProcessor
    {
        ProcessResult Process(FileInfo flowFile, BatchProcess process);

        FileInfo GetNextFlowFile(FlowFileLog log);
    }
}