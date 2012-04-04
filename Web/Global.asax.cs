using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Optimization;

namespace Compilify.Web
{
    public class Application : HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MvcHandler.DisableMvcResponseHeader = true;

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterBundles(BundleTable.Bundles);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Root",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
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
