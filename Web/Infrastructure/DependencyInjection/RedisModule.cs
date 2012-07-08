using Autofac;
using Compilify.Messaging;
using Compilify.Services;
using Compilify.Web.Services;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class RedisModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => RedisConnectionGateway.Current)
                   .SingleInstance()
                   .AsSelf();

            builder.Register(x => x.Resolve<RedisConnectionGateway>().GetConnection())
                   .ExternallyOwned();

            builder.RegisterType<RedisExecutionQueue>().As<IExecutionQueue>();

            builder.RegisterType<RedisMessenger>().As<IMessenger>();
        }
    }
}