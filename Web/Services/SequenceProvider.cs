using System.Threading.Tasks;
using BookSleeve;

namespace Compilify.Web.Services
{
    //public interface ISequenceProvider
    //{
    //    long Next();
    //    Task<long> NextAsync();
    //}

    //public class SequenceProvider : ISequenceProvider
    //{
    //    public SequenceProvider(RedisConnection redisConnectionGateway)
    //    {
    //        connection = redisConnection;
    //    }

    //    private const int Db = 0;
    //    private const string Key = "sequence:url";
    //    private readonly RedisConnection connection;

    //    public long Next()
    //    {
    //        return NextAsync().Result;
    //    }

    //    public Task<long> NextAsync()
    //    {
    //        return connection.Strings.Increment(Db, Key);
    //    }
    //}
}