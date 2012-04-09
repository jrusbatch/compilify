using System.Threading.Tasks;

namespace Compilify.Web.Services
{
    public interface ISequenceProvider
    {
        long Next();
        Task<long> NextAsync();
    }

    public class SequenceProvider : ISequenceProvider
    {
        public SequenceProvider(RedisConnectionGateway redisConnectionGateway)
        {
            gateway = redisConnectionGateway;
        }

        private const int Db = 0;
        private const string Key = "sequence:url";
        private readonly RedisConnectionGateway gateway;

        public long Next()
        {
            return gateway.GetConnection().Wait(NextAsync());
        }

        public Task<long> NextAsync()
        {
            return gateway.GetConnection().Strings.Increment(Db, Key);
        }
    }
}