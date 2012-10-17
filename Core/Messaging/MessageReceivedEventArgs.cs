using System;
using Compilify.Models;

namespace Compilify.Messaging
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(WorkerResult workerResult)
        {
            Result = workerResult;
        }

        public WorkerResult Result { get; private set; }
    }
}