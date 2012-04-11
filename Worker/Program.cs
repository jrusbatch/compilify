using System;
using System.Collections.Generic;
using System.Configuration;
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

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledApplicationException;

            Executer = new CodeExecuter();
            TokenSource = new CancellationTokenSource();

            try
            {
                ClientManager = CreateOpenRedisConnection();
                Client = ClientManager.GetClient();

                var tasks = new Task[4];
                
                for (var i = 0; i < 4; i++) {
                    var task = Task.Factory.StartNew(ProcessQueue, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    task.ContinueWith(OnTaskFaulted, TaskContinuationOptions.OnlyOnFaulted);
                }

                Task.WaitAny(tasks.ToArray(), TokenSource.Token);

                Logger.Debug("Task finished.");
            }
            catch (RedisException ex)
            {
                Logger.ErrorException("An error occured while attempting to access Redis.", ex);
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

        public static void OnUnhandledApplicationException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (e.IsTerminating)
            {
                Logger.FatalException("An unhandled exception is causing the worker to terminate.", exception);
            }
            else
            {
                Logger.ErrorException("An unhandled exception occurred in the worker process.", exception);
            }
        }

        private static void OnTaskFaulted(Task task)
        {
            Logger.ErrorException("An exception occured in the worker task.", task.Exception);
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static CodeExecuter Executer;
        private static CancellationTokenSource TokenSource;
        private static IRedisClientsManager ClientManager;
        private static IRedisClient Client;

        private static void ProcessQueue()
        {
            Logger.Debug("ProcessQueue task started.");

            while (true)
            {
                var message = Client.BlockingDequeueItemFromList("queue:execute", null);
                
                if (TokenSource.IsCancellationRequested)
                {
                    Logger.Error("ProcessQueue task cancelled.");
                    break;
                }

                if (message != null)
                {
                    Logger.Debug("Message received.");

                    var messageBytes = Convert.FromBase64String(message);

                    var command = ExecuteCommand.Deserialize(messageBytes);

                    var result = Executer.Execute(command.Code);

                    var response = JsonConvert.SerializeObject(new { result = result });

                    var listeners = Client.PublishMessage("workers:job-done:" + command.ClientId, response);

                    Logger.Debug("Response published to " + listeners + " listeners.");
                }
            }

            Logger.Debug("ProcessQueue task ending.");
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

            return new PooledRedisClientManager(0, new[] { host });
        }
    }
}
