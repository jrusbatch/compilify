using System;
using System.IO;
using Compilify.Infrastructure;
using ProtoBuf;

namespace Compilify.Serialization
{
    public class ProtobufSerializationProvider : ISerializationProvider
    {
        public byte[] Serialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                return default(T);
            }

            T result;
            using (var memoryStream = new MemoryStream(data))
            {
                result = Deserialize<T>(memoryStream);
                memoryStream.Close();
            }

            return result;
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new InvalidOperationException("The stream cannot be read from.");
            }

            return Serializer.Deserialize<T>(stream);
        }
    }
}
