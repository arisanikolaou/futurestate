using System;
using System.Collections.Generic;
using NLog;

namespace FutureState.Flow.Core
{
    public interface ILoaderState
    {
        List<string> Warnings { get; }
        Logger Logger { get; set; }
        Action Commit { get; set; }
        int Current { get;  }
        DateTime StartTime { get; }
    }
}