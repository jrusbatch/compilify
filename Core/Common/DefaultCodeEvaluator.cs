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

        private readonly IQueue<EvaluateCodeCommand> commandQueue;
        private readonly IMessenger messageBus;

        public Task<WorkerResult> Handle(EvaluateCodeCommand command)
        {
            var tcs = new TaskCompletionSource<WorkerResult>();
            var executionId = command.ExecutionId;

            // Create an anonymous event handler to be called if and when a worker finishes executing our code
            EventHandler<MessageReceivedEventArgs> handler = null;
            handler = (sender, e) =>
            {
                var result = WorkerResult.Deserialize(e.Payload);
                if (result.ExecutionId == executionId)
                {
                    messageBus.MessageReceived -= handler;
                    tcs.TrySetResult(result);
                }
            };

            messageBus.MessageReceived += handler;

            // Queue the command for processing
            commandQueue.EnqueueAsync(command)
                        .ContinueWith(t =>
                        {
                            // If anything goes wrong, stop listening for the completion event and update the task
                            messageBus.MessageReceived -= handler;
                            if (t.IsFaulted)
                            {
                                tcs.TrySetException(t.Exception);
                            }
                            else if (t.IsCanceled)
                            {
                                tcs.TrySetCanceled();
                            }
                        }, TaskContinuationOptions.NotOnRanToCompletion);

            return tcs.Task;
        }
    }
}