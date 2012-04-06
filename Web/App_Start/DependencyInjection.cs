using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using BookSleeve;
using Compilify.Web.Infrastructure.DependencyInjection;
using Compilify.Web.Services;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(Compilify.Web.DependencyInjection), "Initialize")]

namespace Compilify.Web
{
    public static class DependencyInjection
    {
        public static void Initialize()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new MongoDbModule());
            builder.RegisterModule(new RedisModule());
            builder.RegisterModule(new MvcModule());
            
            builder.Register(x => new SequenceProvider(x.Resolve<RedisConnection>()))
                   .AsImplementedInterfaces()
                   .InstancePerHttpRequest();

            builder.RegisterType<PageContentRepository>()
                   .AsImplementedInterfaces()
                   .InstancePerHttpRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}