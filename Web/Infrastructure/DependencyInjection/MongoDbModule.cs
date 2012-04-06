using System.Configuration;
using Autofac;
using Autofac.Integration.Mvc;
using MongoDB.Driver;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class MongoDbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CreateConnection)
                   .InstancePerHttpRequest()
                   .AsSelf();
        }

        private static MongoDatabase CreateConnection(IComponentContext context)
        {
            var connectionString = ConfigurationManager.AppSettings["MONGOLAB_URI"];
            return MongoDatabase.Create(connectionString);
        }
    }
}