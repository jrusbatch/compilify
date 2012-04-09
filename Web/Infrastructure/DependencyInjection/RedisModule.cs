using Autofac;
using BookSleeve;
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
                   .ExternallyOwned()
                   .AsSelf();

            builder.Register(x => x.Resolve<RedisConnection>().GetOpenSubscriberChannel())
                   .ExternallyOwned()
                   .AsSelf();
        }
    }
}