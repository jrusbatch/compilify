using System;
using ProtoBuf;

namespace Compilify.Models
{
    [Serializable]
    [ProtoContract]
    public class ExecutionResult
    {
        [ProtoMember(1)]
        public TimeSpan ProcessorTime { get; set; }

        [ProtoMember(2)]
        public long TotalMemoryAllocated { get; set; }

        [ProtoMember(3)]
        public string ConsoleOutput { get; set; }

        [ProtoMember(4)]
        public string Result { get; set; }
    }
}
