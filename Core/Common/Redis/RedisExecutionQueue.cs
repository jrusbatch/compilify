using System;
using System.Threading.Tasks;

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

        public EvaluateCodeCommand Enqueue(EvaluateCodeCommand message)
        {
            gateway.GetConnection().Wait(EnqueueAsync(message));
            return message;
        }

        public Task<EvaluateCodeCommand> EnqueueAsync(EvaluateCodeCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            var tcs = new TaskCompletionSource<EvaluateCodeCommand>();

            var message = command.GetBytes();
            gateway.GetConnection().Lists.AddLast(db, queue, message)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(command);
                    }
                });

            return tcs.Task;
        }

        public EvaluateCodeCommand Dequeue()
        {
            var message = gateway.GetConnection().Lists.BlockingRemoveFirst(db, new[] { queue }, 0).Result;

            if (message != null)
            {
                return EvaluateCodeCommand.Deserialize(message.Item2);
            }

            return null;
        }

        public Task<EvaluateCodeCommand> DequeueAsync()
        {
            return gateway.GetConnection().Lists.RemoveFirst(db, queue)
                          .ContinueWith(t => EvaluateCodeCommand.Deserialize(t.Result));
        }
    }
}
