using System;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BookSleeve;

namespace Compilify.Worker
{
    public sealed partial class Program : IDisposable
    {
        public Program()
        {
            worker = new Worker();
            connection = CreateConnection();

            connection.Open().ContinueWith(OnConnectionOpen);
        }

        private readonly Worker worker;
        private readonly RedisConnection connection;
        private RedisSubscriberConnection channel;

        private void OnConnectionOpen(Task task)
        {
            channel = connection.GetOpenSubscriberChannel();
            channel.Subscribe("workers:execute", OnMessageReceived);
        }

        private void OnMessageReceived(string key, byte[] message)
        {
            var command = ExecuteCommand.Deserialize(message);

            Console.WriteLine(command.Code);

            var result = worker.ProcessItem(command).GetBytes();

            Console.WriteLine("Execution completed.");

            connection.Publish("workers:job-done", result)
                .ContinueWith(x => Console.WriteLine("Message published to workers:job-done. ({0} subscribers)", x.Result));
        }

        public bool OnConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            var isClosing = false;

            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Console.WriteLine("Application is shutting down!");
                    isClosing = true;
                    break;
            }

            if (isClosing)
            {
                Dispose();
            }

            return true;
        }

        public void Dispose()
        {
            if (channel != null)
            {
                channel.Unsubscribe("workers:execute");
                channel.Dispose();
            }

            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
