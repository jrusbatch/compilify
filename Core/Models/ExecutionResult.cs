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
}
