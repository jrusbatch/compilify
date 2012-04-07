using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BookSleeve;
using SignalR;

namespace Compilify.Web.EndPoints
{
    public class ExecuteEndPoint : PersistentConnection
    {
        public override void Initialize(SignalR.Infrastructure.IDependencyResolver resolver)
        {
            if (redis == null)
            {
                redis = CreateRedisConnection();
            }

            if (redis.State != RedisConnectionBase.ConnectionState.Open)
            {
                redis.Wait(redis.Open());
            }

            if (channel == null)
            {
                channel = redis.GetOpenSubscriberChannel();
                channel.Subscribe("workers:job-done", OnExecutionCompleted);
            }

            base.Initialize(resolver);
        }

        private static RedisConnection redis;
        private static RedisSubscriberConnection channel;

        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            var command = new ExecuteCommand
                          {
                              ClientId = connectionId,
                              Code = data
                          };

            var message = command.GetBytes();

            redis.Publish("workers:execute", message);

            return Send(new { status = "ok" });
        }

        private void OnExecutionCompleted(string key, byte[] message)
        {
            var command = ExecuteCommand.Deserialize(message);

            if (!string.IsNullOrEmpty(command.ClientId))
            {
                Send(command.ClientId, command.Result);
            }
        }

        private static RedisConnection CreateRedisConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"];

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