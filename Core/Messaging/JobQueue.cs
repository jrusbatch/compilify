using System;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using EasyNetQ;

namespace Compilify.Messaging
{
    public class JobQueue : IQueue<EvaluateCodeCommand>
    {
        private readonly IBus bus;

        public JobQueue(IBus messageBus)
        {
            bus = messageBus;
        }

        public void Enqueue(EvaluateCodeCommand message)
        {
            throw new NotImplementedException();
        }

        public Task EnqueueAsync(EvaluateCodeCommand message)
        {
            throw new NotImplementedException();
        }

        public EvaluateCodeCommand Dequeue()
        {
            throw new NotImplementedException();
        }

        public Task<EvaluateCodeCommand> DequeueAsync()
        {
            throw new NotImplementedException();
        }
    }

    //public class RedisExecutionQueue : IQueue<EvaluateCodeCommand>
    //{
    //    private readonly ISerializationProvider serializer;
    //    private readonly RedisConnectionGateway gateway;
    //    private readonly int db;
    //    private readonly string queue;

    //    public RedisExecutionQueue(ISerializationProvider serializationProvider, RedisConnectionGateway redisConnectionGateway, int dbNumber, string queueName)
    //    {
    //        if (serializationProvider == null)
    //        {
    //            throw new ArgumentNullException("serializationProvider");
    //        }

    //        if (redisConnectionGateway == null)
    //        {
    //            throw new ArgumentNullException("redisConnectionGateway");
    //        }

    //        serializer = serializationProvider;
    //        gateway = redisConnectionGateway;
    //        queue = queueName;
    //        db = dbNumber;
    //    }

    //    public void Enqueue(EvaluateCodeCommand message)
    //    {
    //        gateway.GetConnection().Wait(EnqueueAsync(message));
    //    }

    //    public Task EnqueueAsync(EvaluateCodeCommand command)
    //    {
    //        if (command == null)
    //        {
    //            throw new ArgumentNullException("command");
    //        }

    //        var message = serializer.Serialize(command);
    //        return gateway.GetConnection().Lists.AddLast(db, queue, message);
    //    }

    //    public EvaluateCodeCommand Dequeue()
    //    {
    //        var message = gateway.GetConnection().Lists.BlockingRemoveFirst(db, new[] { queue }, 0).Result;
    //        return serializer.Deserialize<EvaluateCodeCommand>(message.Item2);
    //    }

    //    public Task<EvaluateCodeCommand> DequeueAsync()
    //    {
    //        return gateway.GetConnection().Lists.RemoveFirst(db, queue)
    //                      .ContinueWith(t => serializer.Deserialize<EvaluateCodeCommand>(t.Result));
    //    }
    //}
}
