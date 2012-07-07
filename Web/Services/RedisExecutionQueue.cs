using System;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Services;

namespace Compilify.Web.Services
{
    public class RedisExecutionQueue : IExecutionQueue
    {
        public RedisExecutionQueue(RedisConnection redisConnection)
        {
            if (redisConnection == null)
            {
                throw new ArgumentNullException("redisConnection");
            }

            redis = redisConnection;
        }

        private readonly RedisConnection redis;

        public Task<long> QueueForExecution(ExecuteCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var message = command.GetBytes();
            return redis.Lists.AddLast(0, "queue:execute", message);
        }
    }
}
