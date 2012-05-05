using System;

namespace Compilify.Models
{
    [Serializable]
    public class ExecutionResult
    {
        public TimeSpan ProcessorTime { get; set; }
        public long TotalMemoryAllocated { get; set; }
        public string ConsoleOutput { get; set; }
        public object Result { get; set; }
    }

    [Serializable]
    public class WorkerResult
    {
        public DateTime Time { get; set; }

        public long Duration { get; set; }

        public ExecutionResult ExecutionResult { get; set; }
    }

    [Serializable]
    public class SandboxResult
    {
        public string ConsoleOutput { get; set; }
        public object ReturnValue { get; set; }
    }
}
