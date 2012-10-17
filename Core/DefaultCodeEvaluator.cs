using System;
using System.Threading;
using System.Threading.Tasks;
using Compilify.Infrastructure;
using Compilify.LanguageServices;
using Compilify.Messaging;
using Compilify.Models;
using EasyNetQ;

namespace Compilify
{
    public sealed class DefaultCodeEvaluator : ICodeEvaluator
    {
        private readonly Messenger messageBus;

        public DefaultCodeEvaluator(IMessenger messenger)
        {
            messageBus = messenger as Messenger;
        }

        public Task<ICodeRunResult> EvaluateAsync(ICodeProject command, CancellationToken token = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            EvaluateCodeCommand cmd;
            if ((cmd = command as EvaluateCodeCommand) == null)
            {
                throw new ArgumentException("This evaluator can only deal with the type EvaluateCodeCommand");
            }

            var tcs = new TaskCompletionSource<ICodeRunResult>();
            var executionId = cmd.ExecutionId;

            // Create an anonymous event handler to be called if and when a worker finishes executing our code
            EventHandler<IMessage<WorkerResult>> handler = null;
            handler = (sender, e) =>
            {
                token.ThrowIfCancellationRequested();

                if (e.Body.ExecutionId == executionId)
                {
                    messageBus.JobDone -= handler;
                    tcs.TrySetResult(e.Body);
                }
            };

            messageBus.JobDone += handler;

            token.Register(() =>
            {
                messageBus.JobDone -= handler;
                tcs.TrySetCanceled();
            });
            
            // Queue the command for processing
            var task = messageBus.Enqueue(cmd);
            task.ContinueWith(
                t =>
                {
                    // If anything goes wrong, stop listening for the completion event and update the task
                    if (t.IsFaulted)
                    {
                        messageBus.JobDone -= handler;
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