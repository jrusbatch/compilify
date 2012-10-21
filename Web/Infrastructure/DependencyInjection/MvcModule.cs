using System.Linq;
using Autofac;
using Autofac.Integration.Mvc;
using Compilify.Web.Commands;
using Compilify.Web.Queries;

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
                    .Where(x => x.Namespace != null && 
                               (x.Namespace == typeof(IQuery).Namespace ||
                                x.Namespace == typeof(ICommand).Namespace));

            foreach (var query in queries)
            {
                builder.RegisterType(query).AsSelf().InstancePerDependency();
            }
        }
    }
}