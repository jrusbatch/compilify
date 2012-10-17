using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Compilify.Web;
using Compilify.Web.Infrastructure.DependencyInjection;
using SignalR;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(DependencyInjection), "Initialize")]

namespace Compilify.Web
{
    public static class DependencyInjection
    {
        public static void Initialize()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(typeof(Application).Assembly);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalHost.DependencyResolver = new AutofacDepenedencyResolverForSignalR(container);
        }
    }
}