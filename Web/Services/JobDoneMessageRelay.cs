using System;
using System.Web.Hosting;
using BookSleeve;
using Compilify.Models;
using Compilify.Web.EndPoints;
using SignalR;

namespace Compilify.Web.Services
{
    /// <summary>
    /// Processes messages sent by workers over Redis and forwards them to the client
    /// that originally initiated the request.</summary>
    public class JobDoneMessageRelay : IRegisteredObject
    {
        private const string EventKey = "workers:job-done";

        public JobDoneMessageRelay()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public JobDoneMessageRelay(RedisConnectionGateway redisConnectionGateway)
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
        
        /// <summary>
        /// Handle messages received from workers through Redis.</summary>
        /// <param name="key">
        /// The name of the channel on which the message was received.</param>
        /// <param name="message">
        /// A JSON message.</param>
        public void OnMessageRecieved(string key, byte[] message)
        {
            var context = GlobalHost.ConnectionManager.GetConnectionContext<ExecuteEndPoint>();
            var result = WorkerResult.Deserialize(message);

            // Forward the message to the user's browser with SignalR
            context.Connection.Send(result.ClientId, new { status = "ok", data = result.ToResultString() });
        }

        public void Stop(bool immediate)
        {
            channel.Closed -= OnChannelClosed;
            channel.Close(immediate);
            gateway.Close(immediate);
            HostingEnvironment.UnregisterObject(this);
        }
    }
}
