using System;
using System.Linq;
using Autofac;
using Autofac.Integration.Mvc;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class MvcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(Application).Assembly;
            builder.RegisterControllers(assembly);
            builder.RegisterModelBinders(assembly);
            builder.RegisterModelBinderProvider();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterFilterProvider();

            var queries =
                ThisAssembly
                    .GetTypes()
                    .Where(x => x.Namespace != null &&
                                (x.Namespace.StartsWith("Compilify.Web.Queries", StringComparison.Ordinal) ||
                                 x.Namespace.StartsWith("Compilify.Web.Commands", StringComparison.Ordinal)));

            foreach (var query in queries)
            {
                builder.RegisterType(query).AsSelf().InstancePerDependency();
            }
        }
    }
}