using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookSleeve;
using Compilify.Services;
using Newtonsoft.Json;
using Roslyn.Scripting.CSharp;
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

            try
            {
                using (var connection = GetOpenConnection())
                {
                    ProcessQueue(connection, new[] { "queue:execute" });
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("An error occured while processing the queue.", ex);
            }

            Logger.Info("Application ending.");

            return -1; // Return a non-zero code so AppHarbor restarts the worker
        }
        
        private const int DefaultTimeout = 5000;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CodeExecuter Executer = new CodeExecuter();
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        
        private static void ProcessQueue(RedisConnection connection, string[] queues)
        {
            var stopWatch = new Stopwatch();
            var formatter = new ObjectFormatter(maxLineLength: 5120);

            Logger.Info("ProcessQueue task {0} started.", ProcessId);

            while (true)
            {
                var message = connection.Lists.BlockingRemoveFirst(0, queues, DefaultTimeout);

                if (message.Result == null)
                {
                    continue;
                }
                
                var command = ExecuteCommand.Deserialize(message.Result.Item2);

                var timeInQueue = DateTime.UtcNow - command.Submitted;

                Logger.Info("Job received after {0:N3} seconds in queue.", timeInQueue.TotalSeconds);

                if (timeInQueue > command.TimeoutPeriod)
                {
                    Logger.Warn("Job was in queue for longer than {0} seconds, skipping!", command.TimeoutPeriod.Seconds);
                    continue;
                }

                stopWatch.Start();

                var result = Executer.Execute(command.Code, command.Classes);

                stopWatch.Stop();

                Logger.Info("Work completed in {0} milliseconds.", stopWatch.ElapsedMilliseconds);

                var response = JsonConvert.SerializeObject(new
                               {
                                   code = command.Code,
                                   classes = command.Classes,
                                   result = formatter.FormatObject(result), 
                                   time = DateTime.UtcNow,
                                   duration = stopWatch.ElapsedMilliseconds
                               });

                var bytes = Encoding.UTF8.GetBytes(response);
                var listeners = connection.Publish("workers:job-done:" + command.ClientId, bytes);

                Logger.Info("Work results published to {0} listeners.", listeners.Result);

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
    }
}
