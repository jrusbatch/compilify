using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compilify.Services;
using Newtonsoft.Json;
using ServiceStack.Redis;
using NLog;

namespace Compilify.Worker
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            Logger.Info("Application started.");

            Executer = new CodeExecuter();
            TokenSource = new CancellationTokenSource();

            try
            {
                ClientManager = CreateOpenRedisConnection();
                Client = ClientManager.GetClient();

                var task = Task.Factory.StartNew(ProcessQueue, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                
                task.Wait();
            }
            finally
            {
                if (TokenSource != null)
                {
                    TokenSource.Cancel();
                    TokenSource.Dispose();
                }

                if (Client != null)
                {
                    Client.Dispose();
                }

                if (ClientManager != null)
                {
                    ClientManager.Dispose();
                }
            }

            Logger.Info("Application ending.");

            return -1; // Return a non-zero code so AppHarbor restarts the worker
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static CodeExecuter Executer;
        private static CancellationTokenSource TokenSource;
        private static IRedisClientsManager ClientManager;
        private static IRedisClient Client;

        private static void ProcessQueue()
        {
            Logger.Info("ProcessQueue task started.");

            while (true)
            {
                var message = Client.BlockingDequeueItemFromList("queue:execute", null);
                
                if (TokenSource.IsCancellationRequested)
                {
                    Logger.Info("ProcessQueue task cancelled.");
                    break;
                }

                if (message != null)
                {
                    Logger.Info("Message received.");

                    var messageBytes = Convert.FromBase64String(message);

                    var command = ExecuteCommand.Deserialize(messageBytes);

                    var result = Executer.Execute(command.Code);

                    var response = JsonConvert.SerializeObject(new { result = result });

                    Client.PublishMessage("workers:job-done:" + command.ClientId, response);

                    Logger.Info("Response published.");
                }
            }

            Logger.Info("ProcessQueue task ending.");
        }

        private static IRedisClientsManager CreateOpenRedisConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"] ?? "redis://localhost:6379";

            var uri = new Uri(connectionString);
            var password = uri.UserInfo.Split(':').LastOrDefault();

#if !DEBUG
            var host = string.Format("{0}@{1}:{2}", password ?? string.Empty, uri.Host, uri.Port);
#else
            var host = uri.Host;
#endif

            return new BasicRedisClientManager(0, new[] { host });
        }
    }
}
