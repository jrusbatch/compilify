using System;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Infrastructure;
using Compilify.Messaging;
using Compilify.Models;
using Compilify.Serialization;

namespace Compilify.DataAccess.Redis
{
    public class RedisMessenger : IMessenger
    {
        private const string EventKey = "workers:job-done";
        private static RedisSubscriberConnection channel;
        private readonly RedisConnectionGateway gateway;

        private readonly ISerializationProvider serializer;

        public RedisMessenger(RedisConnectionGateway redisConnectionGateway)
            : this(redisConnectionGateway, new ProtobufSerializationProvider()) { }

        public RedisMessenger(RedisConnectionGateway redisConnectionGateway, ISerializationProvider serializationProvider)
        {
            gateway = redisConnectionGateway;
            serializer = serializationProvider;
            Subscribe();
        }
        
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Handle messages received from workers through Redis.</summary>
        /// <param name="key">
        /// The name of the channel on which the message was received.</param>
        /// <param name="message">
        /// A JSON message.</param>
        public void OnMessageRecieved(string key, byte[] message)
        {
            var result = serializer.Deserialize<WorkerResult>(message);

            var eventArgs = new MessageReceivedEventArgs(result);
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

        public Task Publish(WorkerResult result)
        {
            var message = serializer.Serialize(result);

            return gateway.GetConnection().Publish(EventKey, message);
        }

        private void Subscribe()
        {
            channel = gateway.GetConnection().GetOpenSubscriberChannel();
            channel.Closed += OnChannelClosed;

            channel.Subscribe(EventKey, OnMessageRecieved);
        }
    }
}
