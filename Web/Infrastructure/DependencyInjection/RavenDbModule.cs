using Autofac;
using Autofac.Integration.Mvc;
using Raven.Client;
using Raven.Client.Document;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class RavenDbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new DocumentStore { ConnectionStringName = "RavenDB" }.Initialize())
                   .As<IDocumentStore>()
                   .SingleInstance();

            builder.Register(x => x.Resolve<IDocumentStore>().OpenSession())
                   .As<IDocumentSession>()
                   .InstancePerHttpRequest();

            builder.Register(x => x.Resolve<IDocumentStore>().OpenAsyncSession())
                   .As<IAsyncDocumentSession>()
                   .InstancePerHttpRequest();
        }
    }
}