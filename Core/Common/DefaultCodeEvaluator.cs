using System;
using System.Collections.Concurrent;
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
            // messageBus = messenger;
            messenger.MessageReceived += OnMessageReceived;
        }

        // private readonly IMessenger messageBus;
        private readonly IQueue<EvaluateCodeCommand> commandQueue;

        private static readonly ConcurrentDictionary<Guid, TaskCompletionSource<WorkerResult>> outstandingTasks =
            new ConcurrentDictionary<Guid, TaskCompletionSource<WorkerResult>>();

        private static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var result = WorkerResult.Deserialize(e.Payload);

            TaskCompletionSource<WorkerResult> tcs;
            if (outstandingTasks.TryRemove(result.ExecutionId, out tcs))
            {
                tcs.TrySetResult(result);
            }
        }

        public Task<WorkerResult> Handle(EvaluateCodeCommand command)
        {
            var tcs = outstandingTasks.GetOrAdd(command.ExecutionId, _ => new TaskCompletionSource<WorkerResult>());

            return commandQueue.EnqueueAsync(command)
                               .ContinueWith(t =>
                               {
                                   if (t.IsFaulted)
                                   {
                                       tcs.SetException(t.Exception);
                                       RemoveOutstandingTask(command.ExecutionId);
                                   }
                                   else if (t.IsCanceled)
                                   {
                                       tcs.TrySetCanceled();
                                       RemoveOutstandingTask(command.ExecutionId);
                                   }

                                   return tcs.Task;
                               })
                               .Unwrap();
        }

        private static bool RemoveOutstandingTask(Guid executionId)
        {
            TaskCompletionSource<WorkerResult> tcs;
            return outstandingTasks.TryRemove(executionId, out tcs);
        }
    }
}