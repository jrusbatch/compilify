using System;
using System.Threading.Tasks;

namespace Compilify.Messaging
{
    public interface IMessenger
    {
        Task Publish(string channel, byte[] message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
