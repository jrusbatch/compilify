using System;
using System.IO;
using ProtoBuf;

namespace Compilify
{
    [Serializable]
    [ProtoContract]
    public sealed class EvaluateCodeCommand
    {
        [ProtoMember(1)]
        private Guid id = Guid.NewGuid();

        public EvaluateCodeCommand()
        {
        }

        public Guid ExecutionId
        {
            get { return id; }
        }

        [ProtoMember(2)]
        public string ClientId { get; set; }

        [ProtoMember(3)]
        public string Code { get; set; }

        [ProtoMember(4)]
        public string Classes { get; set; }

        [ProtoMember(5)]
        public string Result { get; set; }

        [ProtoMember(6)]
        public DateTime Submitted { get; set; }

        [ProtoMember(7)]
        public TimeSpan TimeoutPeriod { get; set; }
        
        public static EvaluateCodeCommand Deserialize(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<EvaluateCodeCommand>(stream);
            }
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
