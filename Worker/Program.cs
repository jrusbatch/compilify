using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Compilify.Common.Redis;
using Compilify.Messaging;
using Compilify.Messaging.Redis;
using Compilify.Models;
using Compilify.Services;
using Newtonsoft.Json;
using NLog;

namespace Compilify.Worker
{
    public sealed class Program
    {
        private const int DefaultTimeout = 5000;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CSharpExecutor Executer = new CSharpExecutor();

        private static IQueue<EvaluateCodeCommand> queue;
        private static IMessenger messenger;
        
        public static int Main(string[] args)
        {
            Logger.Info("Application started.");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledApplicationException;

            // Log the exception, but do not mark it as observed. The process will be terminated and restarted 
            // automatically by AppHarbor
            TaskScheduler.UnobservedTaskException += 
                (sender, e) => Logger.ErrorException("An unobserved task exception occurred", e.Exception);

            var gateway = new RedisConnectionGateway(ConfigurationManager.AppSettings["REDISTOGO_URL"]);

            queue = new RedisExecutionQueue(gateway, 0, "queue:execute");
            messenger = new RedisMessenger(gateway);

            ProcessQueue();

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

        private static void ProcessQueue()
        {
            var stopWatch = new Stopwatch();

            Logger.Info("ProcessQueue started.");

            while (true)
            {
                var cmd = queue.Dequeue();

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
                                       ExecutionId = cmd.ExecutionId,
                                       ClientId = cmd.ClientId,
                                       Time = DateTime.UtcNow,
                                       Duration = stopWatch.ElapsedMilliseconds,
                                       ExecutionResult = result 
                                   };

                    messenger.Publish("workers:job-done", response.GetBytes());
                }
                catch (JsonSerializationException ex)
                {
                    Logger.ErrorException("An error occurred while attempting to serialize the JSON result.", ex);
                }
                
                stopWatch.Reset();
            }
        }
    }
}
