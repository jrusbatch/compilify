using System;
using System.Threading;
using System.Threading.Tasks;
using Compilify.Infrastructure;
using Compilify.LanguageServices;
using Compilify.Messaging;
using Compilify.Models;

namespace Compilify
{
    public sealed class DefaultCodeEvaluator : ICodeEvaluator
    {
        private readonly ISerializationProvider serializer;
        private readonly IQueue<EvaluateCodeCommand> commandQueue;
        private readonly IMessenger messageBus;

        public DefaultCodeEvaluator(IMessenger messenger, IQueue<EvaluateCodeCommand> messageQueue, ISerializationProvider serializationProvider)
        {
            serializer = serializationProvider;
            commandQueue = messageQueue;
            messageBus = messenger;
        }

        public Task<ICodeRunResult> EvaluateAsync(ICodeProgram command, CancellationToken token)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (!(command is EvaluateCodeCommand))
            {
                throw new ArgumentException("This evaluator can only deal with the type EvaluateCodeCommand");
            }

            var cmd = (EvaluateCodeCommand)command;
            var tcs = new TaskCompletionSource<ICodeRunResult>();
            var executionId = cmd.ExecutionId;

            // Create an anonymous event handler to be called if and when a worker finishes executing our code
            EventHandler<MessageReceivedEventArgs> handler = null;
            handler = (sender, e) =>
            {
                token.ThrowIfCancellationRequested();

                var result = serializer.Deserialize<WorkerResult>(e.Payload);
                if (result.ExecutionId == executionId)
                {
                    messageBus.MessageReceived -= handler;
                    tcs.TrySetResult(result);
                }
            };

            messageBus.MessageReceived += handler;

            token.Register(() =>
            {
                messageBus.MessageReceived -= handler;
                tcs.TrySetCanceled();
            });
            
            // Queue the command for processing
            var task = commandQueue.EnqueueAsync(cmd);

            task.ContinueWith(
                t =>
                {
                    token.ThrowIfCancellationRequested();

                    // If anything goes wrong, stop listening for the completion event and update the task
                    if (t.IsFaulted)
                    {
                        messageBus.MessageReceived -= handler;
                        tcs.TrySetException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                },
                TaskContinuationOptions.NotOnRanToCompletion);

            return tcs.Task;
        }
    }
}