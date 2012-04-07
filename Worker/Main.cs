using System;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using BookSleeve;

namespace Compilify.Worker
{
    public sealed partial class Program
    {
        public static int Main(string[] args)
        {
            using (var connection = CreateConnection())
            {
                if (connection.State == RedisConnectionBase.ConnectionState.Shiny)
                {
                    connection.Wait(connection.Open());
                }

                using (var program = new Program(connection))
                {
                    SetConsoleCtrlHandler(program.OnConsoleCtrlCheck, true);
                    Console.ReadKey();
                }
            }

            return -1;
        }

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static RedisConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"] ?? "redis://localhost";

            var uri = new Uri(connectionString);
            var password = uri.UserInfo.Split(':').Last();
#if !DEBUG
            var connection = new RedisConnection(uri.Host, uri.Port, password: password);
#else
            var connection = new RedisConnection(uri.Host);
#endif
            connection.Wait(connection.Open());
            return connection;
        }
    }
}
