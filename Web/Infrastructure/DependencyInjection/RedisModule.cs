using System;
using System.Configuration;
using System.Linq;
using Autofac;
using Autofac.Integration.Mvc;
using BookSleeve;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class RedisModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => CreateConnection())
                .SingleInstance()
                .AsSelf();

            builder.Register(x => x.Resolve<RedisConnection>().GetOpenSubscriberChannel())
                .SingleInstance()
                .AsSelf();
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
    }
}