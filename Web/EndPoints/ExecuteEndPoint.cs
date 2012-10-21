using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Compilify.Messaging;
using Compilify.Models;
using Compilify.Web.Models;
using Compilify.Utilities;
using MassTransit;
using Newtonsoft.Json;
using SignalR;
using IRequest = SignalR.IRequest;

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
            var viewModel = JsonConvert.DeserializeObject<WorkspaceState>(data);

            var command = new EvaluateCodeCommand
                          {
                              Documents = new List<Document>(viewModel.Project.Documents),

                              ClientId = connectionId,
                              Submitted = DateTime.UtcNow,
                              TimeoutPeriod = ExecutionTimeout
                          };

            Bus.Instance.Publish(command);

            return TaskAsyncHelper.Empty;
        }
    }
}