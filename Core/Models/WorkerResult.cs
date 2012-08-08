using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Compilify.Extensions;

namespace Compilify.Models
{
    [Serializable]
    [DataContract]
    public class WorkerResult
    {
        [DataMember(Order = 1)]
        public Guid ExecutionId { get; set; }
        
        [DataMember(Order = 2)]
        public string ClientId { get; set; }
        
        [DataMember(Order = 3)]
        public DateTime Time { get; set; }
        
        [DataMember(Order = 4)]
        public long Duration { get; set; }

        [DataMember(Order = 5)]
        public ExecutionResult ExecutionResult { get; set; }
        
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
    }
}
