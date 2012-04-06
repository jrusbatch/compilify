using System.Configuration;
using Autofac;
using Autofac.Integration.Mvc;
using BookSleeve;

namespace Compilify.Web.Infrastructure.Modules
{
    public class RedisModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CreateConnection)
                .InstancePerHttpRequest()
                .AsSelf();
        }

        private static RedisConnection CreateConnection(IComponentContext context)
        {
            var connectionString = ConfigurationManager.AppSettings["REDISTOGO_URL"];
            var connection = new RedisConnection(connectionString);
            connection.Wait(connection.Open());
            return connection;
        }
    }
}