using System;
using System.IO;
using System.Text;
using Compilify.Extensions;
using ProtoBuf;

namespace Compilify.Models
{
    [Serializable]
    [ProtoContract]
    public class WorkerResult
    {
        [ProtoMember(1)]
        public Guid ExecutionId { get; set; }
        
        [ProtoMember(2)]
        public string ClientId { get; set; }
        
        [ProtoMember(3)]
        public DateTime Time { get; set; }
        
        [ProtoMember(4)]
        public long Duration { get; set; }

        [ProtoMember(5)]
        public ExecutionResult ExecutionResult { get; set; }
        
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

        public string ToResultString()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(ExecutionResult.ConsoleOutput))
            {
                builder.AppendLine(ExecutionResult.ConsoleOutput);
            }

            builder.AppendLine(ExecutionResult.Result);

            builder.AppendLine();
            builder.AppendFormat("CPU Time: {0}" + Environment.NewLine, ExecutionResult.ProcessorTime);

            builder.AppendFormat(
                "Bytes Allocated: {0}" + Environment.NewLine, ExecutionResult.TotalMemoryAllocated.ToByteSizeString());

            return builder.ToString();
        }
        
        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                return stream.ToArray();
            }
        }
    }
}
