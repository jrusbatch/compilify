using System;
using System.IO;
using System.Runtime.Serialization;

namespace Compilify.LanguageServices
{
    [Serializable]
    [DataContract]
    public sealed class EvaluateCodeCommand : ICodeProgram
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
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public string Language { get; set; }

        [DataMember(Order = 4)]
        public string ClientId { get; set; }

        [DataMember(Order = 5)]
        public string Content { get; set; }

        [DataMember(Order = 6)]
        public string Classes { get; set; }

        [DataMember(Order = 7)]
        public string Result { get; set; }

        [DataMember(Order = 8)]
        public DateTime Submitted { get; set; }

        [DataMember(Order = 9)]
        public TimeSpan TimeoutPeriod { get; set; }
    }
}
