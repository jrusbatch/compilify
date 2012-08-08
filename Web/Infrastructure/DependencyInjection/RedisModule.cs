using System.Configuration;
using Autofac;
using Compilify.Common;
using Compilify.DataAccess.Redis;
using Compilify.LanguageServices;
using Compilify.Messaging;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class RedisModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => new RedisConnectionGateway(ConfigurationManager.AppSettings["REDISTOGO_URL"]))
                   .SingleInstance()
                   .AsSelf();

            builder.Register(x => new RedisExecutionQueue(x.Resolve<RedisConnectionGateway>(), 0, "queue:execute"))
                   .As<IQueue<EvaluateCodeCommand>>();

            builder.RegisterType<RedisMessenger>().As<IMessenger>();

            builder.RegisterType<DefaultCodeEvaluator>()
                   .As<ICodeEvaluator>()
                   .SingleInstance();
        }
    }
}
