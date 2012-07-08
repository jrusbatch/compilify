using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using Compilify.Models;
using Newtonsoft.Json;
using SignalR;

namespace Compilify.Web.EndPoints
{
    public class ExecuteEndPoint : PersistentConnection
    {
        static ExecuteEndPoint()
        {
            int timeout;
            if (!int.TryParse(ConfigurationManager.AppSettings["Compilify.ExecutionTimeout"], out timeout))
            {
                timeout = DefaultExecutionTimeout;
            }

            ExecutionTimeout = TimeSpan.FromSeconds(timeout);
        }

        private const int DefaultExecutionTimeout = 30;

        private static readonly TimeSpan ExecutionTimeout;

        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            var post = JsonConvert.DeserializeObject<Post>(data);

            var command = new ExecuteCommand
                          {
                              ClientId = connectionId,
                              Code = post.Content,
                              Classes = post.Classes,
                              Submitted = DateTime.UtcNow,
                              TimeoutPeriod = ExecutionTimeout
                          };

            var queue = DependencyResolver.Current.GetService<IQueue<ExecuteCommand>>();

            return queue.EnqueueAsync(command)
                        .ContinueWith(t => {
                            if (t.IsFaulted) {
                                return Connection.Send(connectionId, new {
                                    status = "error",
                                    message = t.Exception != null ? t.Exception.Message : null
                                });
                            }

                            return Connection.Send(connectionId, new { status = "ok" });
                        });
        }
    }
}
