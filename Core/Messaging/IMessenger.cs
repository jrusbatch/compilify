using System;
using System.Threading.Tasks;

namespace Compilify.Messaging
{
    public interface IMessenger
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        Task Publish(string channel, byte[] message);
    }
}
