using System.Threading.Tasks;
using BookSleeve;

namespace Compilify.Web.Services
{
    public interface ISequenceProvider
    {
        long Next();
        Task<long> NextAsync();
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

        public long Next()
        {
            return redis.Wait(NextAsync());
        }

        public Task<long> NextAsync()
        {
            return redis.Strings.Increment(Db, Key);
        }
    }
}