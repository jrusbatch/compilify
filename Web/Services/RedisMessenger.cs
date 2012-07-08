using System;
using System.Web.Hosting;
using BookSleeve;
using Compilify.Messaging;

namespace Compilify.Web.Services
{
    public class RedisMessenger : IMessenger, IRegisteredObject
    {
        private const string EventKey = "workers:job-done";

        public RedisMessenger(RedisConnectionGateway redisConnectionGateway)
        {
            HostingEnvironment.RegisterObject(this);
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

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Stop(bool immediate)
        {
            channel.Closed -= OnChannelClosed;
            channel.Close(immediate);
            channel.Dispose();
            gateway.Close(immediate);
            HostingEnvironment.UnregisterObject(this);
        }
    }
}
