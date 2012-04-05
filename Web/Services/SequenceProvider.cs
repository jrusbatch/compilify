using System.Threading.Tasks;
using BookSleeve;

namespace Compilify.Web.Services
{
    public interface ISequenceProvider
    {
        Task<long> Next();
    }

    public class SequenceProvider : ISequenceProvider
    {
        public SequenceProvider(RedisConnection redisConnection)
        {
            redis = redisConnection;
        }

        private const int Db = 0;
        private const string Key = "sequence:url";
        private readonly RedisConnection redis;

        public Task<long> Next()
        {
            return redis.Strings.Increment(Db, Key);
        }
    }
}