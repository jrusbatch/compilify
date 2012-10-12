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
            builder.RegisterControllers(ThisAssembly);
            builder.RegisterModelBinders(ThisAssembly);
            builder.RegisterModelBinderProvider();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterFilterProvider();

            var queries =
                ThisAssembly
                    .GetTypes()
                    .Where(x => x.Namespace != null)
                    .Where(x => x.Namespace.StartsWith("Compilify.Web.Queries", StringComparison.Ordinal));

            foreach (var query in queries)
            {
                builder.RegisterType(query).AsSelf().InstancePerDependency();
            }
        }
    }
}