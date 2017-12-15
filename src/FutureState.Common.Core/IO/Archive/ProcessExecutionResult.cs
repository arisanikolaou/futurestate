using System;
using System.Collections.Generic;

namespace FutureState.IO
{
    public class ProcessExecutionResult
    {
        public int ExitCode { get; set; }

        public IList<string> StandardOutput { get; private set; }

        public IList<string> StandardError { get; private set; }

        public TimeSpan ElapsedTime { get; set; }

        public ProcessExecutionResult()
        {
            StandardOutput = new List<string>();

            StandardError = new List<string>();
        }
    }
}
