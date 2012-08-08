using System;
using System.Runtime.Serialization;

namespace Compilify.Models
{
    [Serializable]
    [DataContract]
    public class ExecutionResult
    {
        [DataMember(Order = 1)]
        public TimeSpan ProcessorTime { get; set; }

        [DataMember(Order = 2)]
        public long TotalMemoryAllocated { get; set; }

        [DataMember(Order = 3)]
        public string ConsoleOutput { get; set; }

        [DataMember(Order = 4)]
        public string Result { get; set; }
    }
}
