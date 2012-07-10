using System;
using System.Threading.Tasks;
using Compilify.Messaging;
using Compilify.Models;

namespace Compilify.Common
{
    public sealed class DefaultCodeEvaluator : ICodeEvaluator
    {
        public DefaultCodeEvaluator(IMessenger messenger, IQueue<EvaluateCodeCommand> messageQueue)
        {
            commandQueue = messageQueue;
            messageBus = messenger;
        }

        private readonly IMessenger messageBus;
        private readonly IQueue<EvaluateCodeCommand> commandQueue;

        public Task<WorkerResult> Handle(EvaluateCodeCommand command)
        {
            var tcs = new TaskCompletionSource<WorkerResult>();

            // Initialize an event handler that will be called after a worker has completed this command.
            EventHandler<MessageReceivedEventArgs> handler = null;
            handler = (sender, e) =>
            {
                // TODO: This approach means every result we get will be deserialized more than once.
                var result = WorkerResult.Deserialize(e.Payload);
                
                if (result.ExecutionId == command.ExecutionId)
                {
                    // Stop listening for messages for workers - this is the message we've been waiting for
                    messageBus.MessageReceived -= handler;
                    tcs.TrySetResult(result);
                }
            };

            messageBus.MessageReceived += handler;

            return commandQueue.EnqueueAsync(command)
                .ContinueWith(t => // ReSharper is complaining about an implicitly captured variable 'command' here
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.Exception);
                        messageBus.MessageReceived -= handler;
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                        messageBus.MessageReceived -= handler;
                    }

                    return tcs.Task;
                })
                .Unwrap();
        }
    }
}