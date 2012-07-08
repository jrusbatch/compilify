using System;

namespace Compilify.Messaging
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string eventKey, byte[] payload)
        {
            EventKey = eventKey;
            Payload = payload;
        }

        public string EventKey { get; private set; }
        public byte[] Payload { get; private set; }
    }
}