using System;
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

            TaskScheduler.UnobservedTaskException += 
                (sender, e) => Logger.ErrorException("An unobserved task exception occurred", e.Exception);

            Executer = new CodeExecuter();
            TokenSource = new CancellationTokenSource();

            try
            {
                var task = Task.Factory.StartNew(ProcessQueue, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                task.ContinueWith(OnTaskFaulted, TaskContinuationOptions.OnlyOnFaulted);

                task.Wait();
                
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

        private static void ProcessQueue()
        {
            Logger.Info("ProcessQueue task {0} started.", Task.CurrentId);

            var stopWatch = new Stopwatch();
            var formatter = new ObjectFormatter(maxLineLength: 5120);

            using (var connection = OpenConnection())
            {
                connection.Error += (sender, e) => Logger.ErrorException(e.Cause, e.Exception);

                connection.Wait(connection.Open());

                while (!TokenSource.IsCancellationRequested)
                {
                    var message = connection.Lists.BlockingRemoveFirst(0, new[] { "queue:execute" }, Int32.MaxValue);

                    var command = ExecuteCommand.Deserialize(message.Result.Item2);

                    stopWatch.Start();

                    var result = Executer.Execute(command.Code, command.Classes);

                    stopWatch.Stop();

                    var response = JsonConvert.SerializeObject(new
                                   {
                                       code = command.Code,
                                       classes = command.Classes,
                                       result = formatter.FormatObject(result), 
                                       time = DateTime.UtcNow,
                                       duration = stopWatch.ElapsedMilliseconds
                                   });

                    var responseBytes = Encoding.UTF8.GetBytes(response);

                    connection.Wait(connection.Publish("workers:job-done:" + command.ClientId, responseBytes));

                    stopWatch.Reset();
                }

                Logger.Error("ProcessQueue task {0} cancelled.", Task.CurrentId);
            }
        }

        private static RedisConnection OpenConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"];
            var uri = new Uri(connectionString);
            var password = uri.UserInfo.Split(':').LastOrDefault();

            if (password != null)
            {
                return new RedisConnection(uri.Host, uri.Port, password: password, syncTimeout: 5000, ioTimeout: 5000);
            }

            return new RedisConnection(uri.Host, uri.Port, syncTimeout: 5000, ioTimeout: 5000);
        }
    }
}
