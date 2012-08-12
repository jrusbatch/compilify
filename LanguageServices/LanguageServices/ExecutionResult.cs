using System;

namespace Compilify.Models
{
    public class ExecutionResult
    {
        public TimeSpan ProcessorTime { get; set; }

        public long TotalMemoryAllocated { get; set; }

        public string ConsoleOutput { get; set; }

        public string Result { get; set; }
    }
}