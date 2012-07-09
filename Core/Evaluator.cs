using System;
using System.Threading.Tasks;
using Compilify.Messaging;
using Compilify.Models;
using Compilify.Utilities;

namespace Compilify
{
    public interface IEvaluator
    {
        Task<WorkerResult> Handle(ExecuteCommand command);
    }

    public sealed class DefaultEvaluator : IEvaluator
    {
        public DefaultEvaluator(IMessenger messenger, IQueue<ExecuteCommand> messageQueue)
        {
            queue = messageQueue;
            bus = messenger;
        }

        private readonly IMessenger bus;
        private readonly IQueue<ExecuteCommand> queue;

        public Task<WorkerResult> Handle(ExecuteCommand command)
        {
            var tcs = new TaskCompletionSource<WorkerResult>();
            EventHandler<MessageReceivedEventArgs> handler = null;

            handler = (sender, e) =>
            {
                // TODO: This approach means every result we get will be deserialized more 
                // than once. So... how about we find another way.
                var result = WorkerResult.Deserialize(e.Payload);
                                
                if (result.ExecutionId == command.ExecutionId)
                {
                    // Unregister the event handler
                    bus.MessageReceived -= handler;
                    tcs.TrySetResult(result);
                }
            };

            bus.MessageReceived += handler;

            return queue.EnqueueAsync(command)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                tcs.SetException(t.Exception);
                                bus.MessageReceived -= handler;
                            }
                            else if (t.IsCanceled)
                            {
                                tcs.TrySetCanceled();
                                bus.MessageReceived -= handler;
                            }

                            return tcs.Task;
                        })
                        .Unwrap();
        }
    }
}
