using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Models;
using Compilify.Services;
using Newtonsoft.Json;
using NLog;

namespace Compilify.Worker
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            Logger.Info("Application started.");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledApplicationException;

            // Log the exception, but do not mark it as observed. The process will be terminated and restarted 
            // automatically by AppHarbor
            TaskScheduler.UnobservedTaskException += 
                (sender, e) => Logger.ErrorException("An unobserved task exception occurred", e.Exception);

            connection = GetOpenConnection();

            ProcessQueue(new[] { "queue:execute" });

            return -1; // Return a non-zero code so AppHarbor restarts the worker
        }
        
        private const int DefaultTimeout = 5000;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CSharpExecutor Executer = new CSharpExecutor();

        private static RedisConnection connection;
        
        private static void ProcessQueue(string[] queues)
        {
            var stopWatch = new Stopwatch();

            Logger.Info("ProcessQueue started.");

            while (true)
            {
                var cmd = WaitForCommandFromQueue(queues);

                if (cmd == null)
                {
                    continue;
                }
                
                var timeInQueue = DateTime.UtcNow - cmd.Submitted;

                Logger.Info("Job received after {0:N3} seconds in queue.", timeInQueue.TotalSeconds);

                if (timeInQueue > cmd.TimeoutPeriod)
                {
                    Logger.Warn("Job was in queue for longer than {0} seconds, skipping!", cmd.TimeoutPeriod.Seconds);
                    continue;
                }

                var post = new Post { Content = cmd.Code, Classes = cmd.Classes };

                stopWatch.Start();

                var result = Executer.Execute(post);

                stopWatch.Stop();

                Logger.Info("Work completed in {0} milliseconds.", stopWatch.ElapsedMilliseconds);

                try
                {
                    var response = new WorkerResult
                                   {
                                       ClientId = cmd.ClientId,
                                       Time = DateTime.UtcNow,
                                       Duration = stopWatch.ElapsedMilliseconds,
                                       ExecutionResult = result 
                                   };

                    Publish(response.GetBytes())
                        .ContinueWith(t => Logger.Info("Work results published to {0} listeners.", t.Result));
                }
                catch (JsonSerializationException ex)
                {
                    Logger.ErrorException("An error occurred while attempting to serialize the JSON result.", ex);
                }
                
                stopWatch.Reset();
            }
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

        private static RedisConnection GetOpenConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"];
            var uri = new Uri(connectionString);
            var password = uri.UserInfo.Split(':').LastOrDefault();

            var conn = new RedisConnection(uri.Host, uri.Port, password: password, allowAdmin: false,
                                           syncTimeout: DefaultTimeout, ioTimeout: DefaultTimeout);

            conn.Error += (sender, e) => Logger.ErrorException(e.Cause, e.Exception);

            conn.Wait(conn.Open());

            return conn;
        }

        private static bool NeedsReset(RedisConnectionBase conn)
        {
            return conn == null || (conn.State != RedisConnectionBase.ConnectionState.Open &&
                                    conn.State != RedisConnectionBase.ConnectionState.Opening);
        }

        private static ExecuteCommand WaitForCommandFromQueue(string[] queues)
        {
            if (NeedsReset(connection))
            {
                if (connection != null)
                {
                    connection.Dispose();
                }

                connection = GetOpenConnection();
            }

            var message = connection.Lists.BlockingRemoveFirst(0, queues, DefaultTimeout);

            if (message.Result != null)
            {
                return ExecuteCommand.Deserialize(message.Result.Item2);
            }

            return null;
        }

        private static Task<long> Publish(byte[] message)
        {
            if (NeedsReset(connection))
            {
                if (connection != null)
                {
                    connection.Dispose();
                }

                connection = GetOpenConnection();
            }

            return connection.Publish("workers:job-done", message);
        }
    }
}
