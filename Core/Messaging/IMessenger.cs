using System;

namespace Compilify.Messaging
{
    public interface IMessenger
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
