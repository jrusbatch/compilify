using System;
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

            if (redis.State != RedisConnectionBase.ConnectionState.Open)
            {
                throw new InvalidOperationException("RedisConnection state is " + redis.State);
            }

            var command = new ExecuteCommand
                            {
                                ClientId = connectionId,
                                Code = data
                            };

            var message = Convert.ToBase64String(command.GetBytes());

            redis.Lists.AddLast(0, "queue:execute", message);

            return Send(new { status = "ok" });
        }
    }
}