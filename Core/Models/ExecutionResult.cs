using System;
using System.IO;
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

    [Serializable]
    [ProtoContract]
    public class WorkerResult
    {
        [ProtoMember(1)]
        public DateTime Time { get; set; }

        [ProtoMember(2)]
        public long Duration { get; set; }

        [ProtoMember(3, AsReference = true)]
        public ExecutionResult ExecutionResult { get; set; }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public static WorkerResult Deserialize(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<WorkerResult>(stream);
            }
        }
    }

    [Serializable]
    public class SandboxResult
    {
        public string ConsoleOutput { get; set; }
        public object ReturnValue { get; set; }
    }
}
