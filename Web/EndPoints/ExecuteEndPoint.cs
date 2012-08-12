using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Compilify.Extensions;
using Compilify.LanguageServices;
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

        public string JobRunToString(ICodeRunResult run)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(run.ConsoleOutput))
            {
                builder.AppendLine(run.ConsoleOutput);
            }

            if (!string.IsNullOrEmpty(run.Result))
            {
                builder.AppendLine(run.Result);
            }

            builder.AppendFormat("CPU Time: {0}" + Environment.NewLine, run.ProcessorTime);
            builder.AppendFormat("Bytes Allocated: {0}" + Environment.NewLine, run.TotalMemoryAllocated.ToByteSizeString());

            return builder.ToString();
        }

        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            var command = JsonConvert.DeserializeObject<EvaluateCodeCommand>(data);
            command.ClientId = connectionId;
            command.Submitted = DateTime.UtcNow;
            command.TimeoutPeriod = ExecutionTimeout;

            // TODO: Time this out
            var tokenSource = new CancellationTokenSource();

            var evaluator = GlobalHost.DependencyResolver.Resolve<ICodeEvaluator>();

            return evaluator
                .EvaluateAsync(command, tokenSource.Token)
                .ContinueWith(
                    t =>
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
                                data = JobRunToString(t.Result)
                            });
                    });
        }
    }
}