using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Compilify.Web.Services;

namespace Compilify.Web
{
    public class Application : System.Web.HttpApplication
    {
        protected static JobDoneMessageRelay MessageRelay;

        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MessageRelay = new JobDoneMessageRelay(DependencyResolver.Current.GetService<RedisConnectionGateway>());
        }
    }
}
