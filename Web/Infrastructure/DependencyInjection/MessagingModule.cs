using Autofac;
using MassTransit;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => Bus.Instance).As<IServiceBus>().SingleInstance();
        }
    }
}