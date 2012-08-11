using Autofac;
using Compilify.LanguageServices;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class RoslynModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CSharpCompiler>().As<ICodeCompiler>();
            builder.RegisterType<CSharpValidator>().As<ICodeValidator>();
        }
    }
}