using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Web.Infrastructure;
using Microsoft.Web.Optimization;
using SignalR.Hosting.AspNet.Routing;

namespace Compilify.Web
{
    public class Application : HttpApplication
    {
        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RegisterBundles(BundleTable.Bundles);
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapConnection<CompilerConnection>("compile", "compile/{*operation}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        private static void RegisterBundles(BundleCollection bundles)
        {
            var css = new Bundle("~/css", typeof(CssMinify));
            css.AddDirectory("~/assets/css", "*.css", false);
            bundles.Add(css);

            var js = new Bundle("~/js", typeof(JsMinify));
            js.AddDirectory("~/assets/js", "*.js", false);
            bundles.Add(js);
        }
    }
}