using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compilify.Services;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Compilify.Worker
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            Executer = new CodeExecuter();
            TokenSource = new CancellationTokenSource();

            try
            {
                ClientManager = CreateOpenRedisConnection();
                Client = ClientManager.GetClient();

                Task.Factory.StartNew(ProcessQueue, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                
                Console.ReadKey();
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

            return -1;
        }

        private static CodeExecuter Executer;
        private static CancellationTokenSource TokenSource;
        private static IRedisClientsManager ClientManager;
        private static IRedisClient Client;

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        private static void ProcessQueue()
        {
            while (true)
            {
                if (TokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Cancellation requested, exiting.");
                    break;
                }

                var message = Client.BlockingDequeueItemFromList("queue:execute", DefaultTimeout);

                if (message != null)
                {
                    Console.WriteLine("Message received: {0}", message);

                    var bytes = Convert.FromBase64String(message);

                    var command = ExecuteCommand.Deserialize(bytes);

                    var result = Executer.Execute(command.Code);

                    var response = JsonConvert.SerializeObject(new { result = result });

                    Console.WriteLine(response);

                    Client.PublishMessage("workers:job-done:" + command.ClientId, response);
                }
            }
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

    public class PeonState
    {
        public PeonState(IRedisClient redisClient, CancellationToken cancellationToken)
        {
            client = redisClient;
            token = cancellationToken;
        }

        private readonly IRedisClient client;
        private readonly CancellationToken token;

        public IRedisClient Client
        {
            get { return client; }
        }

        public CancellationToken CancellationToken
        {
            get { return token; }
        }
    }
}
