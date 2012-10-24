using System;

namespace Compilify.Messaging
{
    [Serializable]
    public class WorkerResult : ICodeRunResult
    {
        public string ClientId { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset StopTime { get; set; }

        public TimeSpan RunDuration { get; set; }

        public TimeSpan ProcessorTime { get; set; }

        public long TotalMemoryAllocated { get; set; }

        public string ConsoleOutput { get; set; }

        public string Result { get; set; }
    }
}