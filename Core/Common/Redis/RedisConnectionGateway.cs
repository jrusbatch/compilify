using System;
using System.Linq;
using System.Net.Sockets;
using BookSleeve;

namespace Compilify.Common.Redis
{
    /// <summary>
    /// Maintains an open connection to a Redis server.</summary>
    /// <remarks>
    /// http://stackoverflow.com/a/8777999/145831 </remarks>
    public sealed class RedisConnectionGateway
    {
        private const string RedisConnectionFailed = "Redis connection failed.";
        private static readonly object SyncConnectionLock = new object();
        private readonly string connectionString;
        private RedisConnection connection;
        
        public RedisConnectionGateway(string connectionString)
        {
            this.connectionString = connectionString;
            connection = CreateConnection();
        }
        
        public RedisConnection GetConnection()
        {
            lock (SyncConnectionLock)
            {
                if (connection == null)
                {
                    connection = CreateConnection();
                }

                if (connection.State == RedisConnectionBase.ConnectionState.Opening)
                {
                    return connection;
                }

                if (connection.State > RedisConnectionBase.ConnectionState.Open)
                {
                    try
                    {
                        connection.Dispose();
                        connection = CreateConnection();
                    }
                    catch (Exception ex)
                    {
                        throw new RedisConnectionException(RedisConnectionFailed, ex);
                    }
                }

                if (connection.State == RedisConnectionBase.ConnectionState.Shiny)
                {
                    try
                    {
                        var openAsync = connection.Open();
                        connection.Wait(openAsync);
                    }
                    catch (SocketException ex)
                    {
                        throw new RedisConnectionException(RedisConnectionFailed, ex);
                    }
                }

                return connection;
            }
        }

        public void Close(bool immediate)
        {
            connection.Close(immediate);
            connection.Dispose();
            connection = null;
        }

        private RedisConnection CreateConnection()
        {
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
