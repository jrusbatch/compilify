using System;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using Compilify.Models;
using EasyNetQ;
using EasyNetQ.Topology;

namespace Compilify.Messaging
{
    public interface IMessenger
    {
    }

    public class Messenger : IMessenger
    {
        private static readonly Task EmptyTask = Task.FromResult<object>(null);

        private readonly IAdvancedBus bus;
        private readonly IExchange jobReadyExchange;
        private readonly IExchange jobDoneExchange;

        public Messenger(IAdvancedBus messageBus)
        {
            bus = messageBus;

            var jobReadyQueue = Queue.DeclareDurable("queue:worker:job-ready");
            jobReadyExchange = Exchange.DeclareDirect("exchange:worker:job-ready");
            jobReadyQueue.BindTo(jobReadyExchange, "job-ready");

            var jobDoneQueue = Queue.DeclareDurable("queue:worker:job-done");
            jobDoneExchange = Exchange.DeclareFanout("exchange:worker:job-done");
            jobDoneQueue.BindTo(jobDoneExchange, "job-done");

            bus.Subscribe<EvaluateCodeCommand>(jobReadyQueue, OnJobReady);
            bus.Subscribe<WorkerResult>(jobDoneQueue, OnJobDone);
        }

        public event EventHandler<IMessage<EvaluateCodeCommand>> JobReady; 
        public event EventHandler<IMessage<WorkerResult>> JobDone; 

        public Task Enqueue(EvaluateCodeCommand command)
        {
            return Publish(jobReadyExchange, "job-ready", command);
        }

        public Task Publish(WorkerResult message)
        {
            return Publish(jobDoneExchange, "job-done", message);
        }

        private Task Publish<T>(IExchange exchange, string filter, T message)
        {
            try
            {
                using (var channel = bus.OpenPublishChannel())
                {
                    channel.Publish(exchange, filter, new Message<T>(message));
                }
            }
            catch (EasyNetQException ex)
            {
                throw new CompilifyException("An error occured while attempting to publish a message", ex);
            }

            return EmptyTask;
        }

        private Task OnJobReady(IMessage<EvaluateCodeCommand> message, MessageReceivedInfo info)
        {
            var handler = JobReady;
            if (handler != null)
            {
                handler(this, message);
            }

            return EmptyTask;
        }

        private Task OnJobDone(IMessage<WorkerResult> message, MessageReceivedInfo info)
        {
            var handler = JobDone;
            if (handler != null)
            {
                handler(this, message);
            }

            return EmptyTask;
        }
    }
}
