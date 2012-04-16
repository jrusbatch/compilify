using System;
using System.IO;
using ProtoBuf;

namespace Compilify
{
    [Serializable]
    [ProtoContract]
    public sealed class ExecuteCommand
    {
        public ExecuteCommand() { }

        public ExecuteCommand(string clientId, string code)
        {
            ClientId = clientId;
            Code = code;
        }

        [ProtoMember(1)]
        public string ClientId { get; set; }

        [ProtoMember(2)]
        public string Code { get; set; }

        [ProtoMember(3)]
        public string Classes { get; set; }

        [ProtoMember(4)]
        public string Result { get; set; }

        public byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public static ExecuteCommand Deserialize(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<ExecuteCommand>(stream);
            }
        }
    }
}
