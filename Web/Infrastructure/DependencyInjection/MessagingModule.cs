using Autofac;
using Compilify.Messaging;
using EasyNetQ;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => RabbitHutch.CreateBus("host=localhost;prefetchcount=1;").Advanced)
                   .SingleInstance()
                   .As<IAdvancedBus>();

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
        }
    }
}