using Compilify.Messaging;
using Compilify.Models;
using Compilify.Web.EndPoints;
using SignalR;

namespace Compilify.Web.Services
{
    /// <summary>
    /// Processes messages sent by workers over Redis and forwards them to the client
    /// that originally initiated the request.</summary>
    public class JobDoneMessageRelay
    {
        public JobDoneMessageRelay(IMessenger messenger)
        {
            messenger.MessageReceived += OnMessageRecieved;
        }

        /// <summary>
        /// Handle messages received from workers through Redis.</summary>
        public void OnMessageRecieved(object sender, MessageReceivedEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetConnectionContext<ExecuteEndPoint>();
            var result = WorkerResult.Deserialize(e.Payload);

            // Forward the message to the user's browser with SignalR
            context.Connection.Send(result.ClientId, new { status = "ok", data = result.ToResultString() });
        }
    }
}
