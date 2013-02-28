using System;
using System.Configuration;
using System.Diagnostics;
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
            var connectionString = ConfigurationManager.AppSettings["CLOUDAMQP_URL"].Replace("amqp://", "rabbitmq://");
            var queueName = ConfigurationManager.AppSettings["Compilify.WebMessagingQueue"];

            var connectionUri = new Uri(connectionString + "/" + queueName);

            var userInfo = connectionUri.UserInfo;

            var queueUri = connectionUri.ToString();
            string username = null;
            string password = null;

            if (!string.IsNullOrEmpty(userInfo))
            {
                queueUri = queueUri.Replace(userInfo + "@", string.Empty);
                var credentials = userInfo.Split(new[] { ':' });
                username = credentials[0];
                password = credentials[1];
            }

            var endpointAddress = string.Format("{0}/{1}", connectionString, queueName);

            Trace.WriteLine("queueUri: " + queueUri);
            Trace.WriteLine("EndpointAddress: " + endpointAddress);
            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq(x => x.ConfigureHost(new Uri(queueUri), y =>
                                                                        {
                                                                            if (!string.IsNullOrEmpty(username))
                                                                            {
                                                                                y.SetUsername(username);
                                                                            }
                                                                                
                                                                            if (!string.IsNullOrEmpty(password))
                                                                            {
                                                                                y.SetPassword(password);
                                                                            }
                                                                        }));
                sbc.ReceiveFrom(endpointAddress);

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