using System;
using System.Threading.Tasks;
using Compilify.LanguageServices;

namespace Compilify.Common.Redis
{
    public class RedisExecutionQueue : IQueue<EvaluateCodeCommand>
    {
        private readonly RedisConnectionGateway gateway;
        private readonly int db;
        private readonly string queue;

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

        public void Enqueue(EvaluateCodeCommand message)
        {
            gateway.GetConnection().Wait(EnqueueAsync(message));
        }

        public Task EnqueueAsync(EvaluateCodeCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var message = command.GetBytes();
            return gateway.GetConnection().Lists.AddLast(db, queue, message);
        }

        public EvaluateCodeCommand Dequeue()
        {
            var message = gateway.GetConnection().Lists.BlockingRemoveFirst(db, new[] { queue }, 0).Result;
            return EvaluateCodeCommand.Deserialize(message.Item2);
        }

        public Task<EvaluateCodeCommand> DequeueAsync()
        {
            return gateway.GetConnection().Lists.RemoveFirst(db, queue)
                          .ContinueWith(t => EvaluateCodeCommand.Deserialize(t.Result));
        }
    }
}
