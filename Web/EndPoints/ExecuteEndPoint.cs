using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookSleeve;
using SignalR;

namespace Compilify.Web.EndPoints
{
    public class ExecuteEndPoint : PersistentConnection
    {
        /// <summary>
        /// Handle messages sent by the client.</summary>
        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            var redis = DependencyResolver.Current.GetService<RedisConnection>();

            var command = new ExecuteCommand
                            {
                                ClientId = connectionId,
                                Code = data
                            };

            var message = command.GetBytes();

            redis.Publish("workers:execute", message);

            return Send(new { status = "ok" });
        }
    }
}