using System.Threading.Tasks;
using System.Web.Mvc;
using Compilify.Models;
using Compilify.Web.Services;
using Newtonsoft.Json;
using SignalR;

namespace Compilify.Web.EndPoints
{
    public class ExecuteEndPoint : PersistentConnection
    {
        /// <summary>
        /// Handle messages sent by the client.</summary>
        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            var post = JsonConvert.DeserializeObject<Post>(data);

            var command = new ExecuteCommand
                          {
                              ClientId = connectionId,
                              Code = post.Content,
                              Classes = post.Classes
                          };

            var message = command.GetBytes();

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