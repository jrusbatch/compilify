using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using Compilify.Messaging;
using Compilify.Models;
using EasyNetQ;
using Newtonsoft.Json;
using NLog;

namespace Compilify.Worker
{
    public sealed class Program
    {
        private const int DefaultTimeout = 5000;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ICodeCompiler Compiler = new CSharpCompiler();

        private static Messenger messenger;

        private static Thread thread;
        private static ManualResetEvent reset;

        public static int Main(string[] args)
        {
            Logger.Info("Application started.");

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledApplicationException;

            // Log the exception, but do not mark it as observed. The process will be terminated and restarted 
            // automatically by AppHarbor
            TaskScheduler.UnobservedTaskException +=
                (sender, e) => Logger.ErrorException("An unobserved task exception occurred", e.Exception);

            reset = new ManualResetEvent(false);

            var bus = RabbitHutch.CreateBus("host=localhost;").Advanced;

            messenger = new Messenger(bus);

            messenger.JobReady += (sender, e) => ProcessCommand(e);

            reset.WaitOne();

            return -1; // Return a non-zero code so AppHarbor restarts the worker
        }

        private static void ProcessCommand(IMessage<EvaluateCodeCommand> command)
        {
            var cmd = command.Body;

            if (cmd == null)
            {
                return;
            }

            var stopWatch = new Stopwatch();

            var timeInQueue = DateTime.UtcNow - cmd.Submitted;

            Logger.Info("Job received after {0:N3} seconds in queue.", timeInQueue.TotalSeconds);

            if (timeInQueue > cmd.TimeoutPeriod)
            {
                Logger.Warn("Job was in queue for longer than {0} seconds, skipping!", cmd.TimeoutPeriod.Seconds);
                return;
            }

            var startedOn = DateTime.UtcNow;
            stopWatch.Start();

            var assembly = Compiler.Compile(cmd);

            ExecutionResult result;
            if (assembly == null)
            {
                result = new ExecutionResult
                            {
                                Result = "[compiling of code failed]"
                            };
            }
            else
            {
                using (var executor = new Sandbox())
                {
                    result = executor.Execute(assembly, cmd.TimeoutPeriod);
                }
            }

            stopWatch.Stop();
            var stoppedOn = DateTime.UtcNow;

            Logger.Info("Work completed in {0} milliseconds.", stopWatch.ElapsedMilliseconds);

            try
            {
                var response = new WorkerResult
                                {
                                    ExecutionId = cmd.ExecutionId,
                                    ClientId = cmd.ClientId,
                                    StartTime = startedOn,
                                    StopTime = stoppedOn,
                                    RunDuration = stopWatch.Elapsed,
                                    ProcessorTime = result.ProcessorTime,
                                    TotalMemoryAllocated = result.TotalMemoryAllocated,
                                    ConsoleOutput = result.ConsoleOutput,
                                    Result = result.Result
                                };

                messenger.Publish(response);
            }
            catch (JsonSerializationException ex)
            {
                Logger.ErrorException("An error occurred while attempting to serialize the JSON result.", ex);
            }

            stopWatch.Reset();
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
    }
}
