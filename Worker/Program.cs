using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using BookSleeve;
using Compilify.Services;
using MongoDB.Driver;

namespace Compilify.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var executer = new CodeExecuter();
            var redis = CreateConnection();

            try
            {
                if (redis.State != RedisConnectionBase.ConnectionState.Open)
                {
                    Console.WriteLine("Connection state is {0}, attempting to open.", redis.State.ToString());
                    redis.Wait(redis.Open());
                }


                while (true)
                {
                    var next = redis.Wait(redis.Lists.RemoveFirstString(0, "queue:executer"));

                    if (!string.IsNullOrEmpty(next))
                    {
                        Console.WriteLine(next);
                        var result = executer.Execute(next);
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("No data found, sleeping.");
                    }

                    Thread.Sleep(3000);
                }
            }
            finally
            {
                redis.Close(false);
                redis.Dispose();
            }
        }

        private static RedisConnection CreateConnection()
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

        private static MongoDatabase CreateDbConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["MONGOLAB_URI"];
            return MongoDatabase.Create(connectionString);
        }
    }
}
