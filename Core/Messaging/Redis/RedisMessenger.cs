using System;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Common.Redis;

namespace Compilify.Messaging.Redis
{
    public class RedisMessenger : IMessenger
    {
        private const string EventKey = "workers:job-done";

        public RedisMessenger(RedisConnectionGateway redisConnectionGateway)
        {
            gateway = redisConnectionGateway;
            Subscribe();
        }

        private readonly RedisConnectionGateway gateway;
        private static RedisSubscriberConnection channel;
        
        private void Subscribe()
        {
            channel = gateway.GetConnection().GetOpenSubscriberChannel();
            channel.Closed += OnChannelClosed;

            channel.Subscribe(EventKey, OnMessageRecieved);
        }

        /// <summary>
        /// Handle messages received from workers through Redis.</summary>
        /// <param name="key">
        /// The name of the channel on which the message was received.</param>
        /// <param name="message">
        /// A JSON message.</param>
        public void OnMessageRecieved(string key, byte[] message)
        {
            var eventArgs = new MessageReceivedEventArgs(key, message);
            MessageReceived(this, eventArgs);
        }
        
        public void OnChannelClosed(object sender, EventArgs e)
        {
            if (channel != null)
            {
                channel.Closed -= OnChannelClosed;
                channel.Dispose();
                channel = null;
            }

            Subscribe();
        }

        public Task Publish(string eventKey, byte[] message)
        {
            return gateway.GetConnection().Publish(eventKey, message);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
