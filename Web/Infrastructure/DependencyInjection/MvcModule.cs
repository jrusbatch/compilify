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
        }
    }
}