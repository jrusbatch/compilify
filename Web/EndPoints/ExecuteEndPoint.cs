using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Compilify.LanguageServices;
using Compilify.Models;
using Newtonsoft.Json;
using SignalR;

namespace Compilify.Web.EndPoints
{
    public class ExecuteEndPoint : PersistentConnection
    {
        private const int DefaultExecutionTimeout = 30;
        private static readonly TimeSpan ExecutionTimeout;
        
        static ExecuteEndPoint()
        {
            int timeout;
            if (!int.TryParse(ConfigurationManager.AppSettings["Compilify.ExecutionTimeout"], out timeout))
            {
                timeout = DefaultExecutionTimeout;
            }

            ExecutionTimeout = TimeSpan.FromSeconds(timeout);
        }

        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            var post = JsonConvert.DeserializeObject<Post>(data);

            var command = new EvaluateCodeCommand
                          {
                              ClientId = connectionId,
                              Code = post.Content,
                              Classes = post.Classes,
                              Submitted = DateTime.UtcNow,
                              TimeoutPeriod = ExecutionTimeout
                          };

            var tokenSource = new CancellationTokenSource();

            var evaluator = DependencyResolver.Current.GetService<ICodeEvaluator>();

            return evaluator.Handle(command, tokenSource.Token)
                            .ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    return Connection.Send(
                                        connectionId,
                                        new
                                        {
                                            status = "error",
                                            message = t.Exception != null ? t.Exception.Message : null
                                        });
                                }
                                
                                if (t.IsCanceled)
                                {
                                    return Connection.Send(
                                        connectionId,
                                        new
                                        {
                                            status = "error",
                                            message = "Evaluation timed out or was cancelled."
                                        });
                                }

                                return Connection.Send(
                                    connectionId,
                                    new
                                    {
                                        status = "ok",
                                        data = t.Result.ToResultString()
                                    });
                            });
        }
    }
}
