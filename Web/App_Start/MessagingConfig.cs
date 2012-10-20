using System;
using System.Configuration;
using System.Text;
using Compilify.Extensions;
using Compilify.Models;
using Compilify.Web.EndPoints;
using MassTransit;
using SignalR;

namespace Compilify.Web
{
    public static class MessagingConfig
    {
        public static void ConfigureServiceBus()
        {
            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(ConfigurationManager.AppSettings["CLOUDAMQP_URL"]);
            });

            Bus.Instance.SubscribeHandler<WorkerResult>(x =>
            {
                var endpoint = GlobalHost.ConnectionManager.GetConnectionContext<ExecuteEndPoint>();
                endpoint.Connection.Send(x.ClientId, new { status = "ok", data = JobRunToString(x) });
            });
        }

        private static string JobRunToString(ICodeRunResult run)
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
    }
}