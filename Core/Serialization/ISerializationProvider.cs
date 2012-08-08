using System.IO;

namespace Compilify.Infrastructure
{
    public interface ISerializationProvider
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);

        T Deserialize<T>(Stream stream);
    }
}
