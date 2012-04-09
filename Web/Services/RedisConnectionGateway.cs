using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using BookSleeve;

namespace Compilify.Web.Services
{
    /// <summary>
    /// Maintains an open connection to a Redis server.</summary>
    /// <remarks>
    /// http://stackoverflow.com/a/8777999/145831 </remarks>
    public sealed class RedisConnectionGateway
    {
        private RedisConnectionGateway()
        {
            connection = CreateConnection();

            connection.Closed += OnConnectionClosed;
        }

        private const string RedisConnectionFailed = "Redis connection failed.";
        private RedisConnection connection;
        private static volatile RedisConnectionGateway instance;

        private static readonly object syncLock = new object();
        private static readonly object syncConnectionLock = new object();

        public event EventHandler ConnectionReset;

        public static RedisConnectionGateway Current
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new RedisConnectionGateway();
                        }
                    }
                }

                return instance;
            }
        }

        private static RedisConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"];
            var uri = new Uri(connectionString);
            var password = uri.UserInfo.Split(':').LastOrDefault();

            try
            {
                if (password != null)
                {
                    return new RedisConnection(uri.Host, uri.Port, password: password, syncTimeout: 5000, ioTimeout: 5000);
                }

                return new RedisConnection(uri.Host, uri.Port, syncTimeout: 5000, ioTimeout: 5000);
            }
            catch (RedisException ex)
            {
                throw new RedisConnectionException(RedisConnectionFailed, ex);
            }
        }

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            lock (syncConnectionLock)
            {
                // Not sure if this is necessary
                connection.Closed -= OnConnectionClosed;
                connection.Dispose();

                connection = CreateConnection();

                ConnectionReset(this, new EventArgs());
            }
        }

        public RedisSubscriberConnection GetSubscriberConnection()
        {
            return GetConnection().GetOpenSubscriberChannel();
        }

        public RedisConnection GetConnection()
        {
            lock (syncConnectionLock)
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
    }
}