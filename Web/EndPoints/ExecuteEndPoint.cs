using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Compilify.Web.Services;
using SignalR;

namespace Compilify.Web.EndPoints {

    public class ExecuteEndPoint : PersistentConnection {

        /// <summary>
        /// Handle messages sent by the client.</summary>
        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            var command = new ExecuteCommand
                            {
                                ClientId = connectionId,
                                Code = data
                            };

            var message = Convert.ToBase64String(command.GetBytes());

            var gateway = DependencyResolver.Current.GetService<RedisConnectionGateway>();
            var redis = gateway.GetConnection();

            return redis.Lists.AddLast(0, "queue:execute", message)
                              .ContinueWith(t => {
                                  if (t.IsFaulted) {
                                      return Send(new {
                                          status = "error",
                                          message = t.Exception != null ? t.Exception.Message : null
                                      });
                                  }

                                  return Send(new { status = "ok" });
                              });
        }
    }
}