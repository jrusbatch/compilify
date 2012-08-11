using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Compilify.Extensions;

namespace Compilify.Models
{
	[Serializable]
	[DataContract]
	public class WorkerResult : ICodeRunResult
	{
		[DataMember(Order = 1)]
		public Guid ExecutionId { get; set; }

		[DataMember(Order = 2)]
		public string ClientId { get; set; }

		[DataMember(Order = 3)]
		public DateTime StartTime { get; set; }

		[DataMember(Order = 4)]
		public DateTime StopTime { get; set; }

		[DataMember(Order = 5)]
		public TimeSpan RunDuration { get; set; }

		[DataMember(Order = 6)]
		public TimeSpan ProcessorTime { get; set; }

		[DataMember(Order = 7)]
		public long TotalMemoryAllocated { get; set; }

		[DataMember(Order = 8)]
		public string ConsoleOutput { get; set; }

		[DataMember(Order = 9)]
		public string Result { get; set; }
	}
}