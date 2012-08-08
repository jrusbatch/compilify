using System;
using System.IO;
using System.Runtime.Serialization;

namespace Compilify.LanguageServices
{
    [Serializable]
    [DataContract]
    public sealed class EvaluateCodeCommand
    {
        [DataMember(Order = 1)]
        private Guid id = Guid.NewGuid();

        public EvaluateCodeCommand()
        {
        }

        public Guid ExecutionId
        {
            get { return id; }
        }

        [DataMember(Order = 2)]
        public string ClientId { get; set; }

        [DataMember(Order = 3)]
        public string Code { get; set; }

        [DataMember(Order = 4)]
        public string Classes { get; set; }

        [DataMember(Order = 5)]
        public string Result { get; set; }

        [DataMember(Order = 6)]
        public DateTime Submitted { get; set; }

        [DataMember(Order = 7)]
        public TimeSpan TimeoutPeriod { get; set; }
    }
}
