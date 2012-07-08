using System;
using System.Threading.Tasks;

namespace Compilify.Common.Redis
{
    public class RedisExecutionQueue : IQueue<ExecuteCommand>
    {
        public RedisExecutionQueue(RedisConnectionGateway redisConnectionGateway, int dbNumber, string queueName)
        {
            if (redisConnectionGateway == null)
            {
                throw new ArgumentNullException("redisConnectionGateway");
            }

            gateway = redisConnectionGateway;
            queue = queueName;
            db = dbNumber;
        }

        private readonly RedisConnectionGateway gateway;
        private readonly int db;
        private readonly string queue;

        public void Enqueue(ExecuteCommand message)
        {
            gateway.GetConnection().Wait(EnqueueAsync(message));
        }

        public Task EnqueueAsync(ExecuteCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var message = command.GetBytes();
            return gateway.GetConnection().Lists.AddLast(db, queue, message);
        }

        public ExecuteCommand Dequeue()
        {
            var message = gateway.GetConnection().Lists.BlockingRemoveFirst(db, new[] { queue }, 0).Result;

            if (message != null)
            {
                return ExecuteCommand.Deserialize(message.Item2);
            }

            return null;
        }

        public Task<ExecuteCommand> DequeueAsync()
        {
            return gateway.GetConnection().Lists.RemoveFirst(db, queue)
                          .ContinueWith(t => ExecuteCommand.Deserialize(t.Result));
        }
    }
}
