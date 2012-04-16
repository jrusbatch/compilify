using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Compilify.Web.EndPoints;
using Compilify.Web.Infrastructure.Extensions;
using Compilify.Web.Services;
using SignalR.Hosting.AspNet.Routing;

namespace Compilify.Web {

    public class Application : HttpApplication {
        
        public static JobDoneMessageRelay MessageRelay;

        protected void Application_Start() {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MvcHandler.DisableMvcResponseHeader = true;

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterBundles(BundleTable.Bundles);
            RegisterRoutes(RouteTable.Routes);
            MessageRelay = new JobDoneMessageRelay(DependencyResolver.Current.GetService<RedisConnectionGateway>());
        }
        
        private static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapLowercaseRoute(
                name: "Root",
                url: "",
                defaults: new { controller = "Home", action = "Index" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapLowercaseRoute(
                name: "About",
                url: "about",
                defaults: new { controller = "Home", action = "About" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapLowercaseRoute(
                name: "validate",
                url: "validate",
                defaults: new { controller = "Home", action = "Validate" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapConnection<ExecuteEndPoint>("execute", "execute/{*operation}");

            routes.MapLowercaseRoute(
                name: "Update",
                url: "{slug}/{version}",
                defaults: new { controller = "Home", action = "Save", version = UrlParameter.Optional },
                constraints: new {
                                 httpMethod = new HttpMethodConstraint("POST"),
                                 slug = @"[a-z0-9]*"
                             }
            );

            routes.MapLowercaseRoute(
                name: "Save",
                url: "{slug}",
                defaults: new { controller = "Home", action = "Save", slug = UrlParameter.Optional },
                constraints: new {
                                 httpMethod = new HttpMethodConstraint("POST"),
                                 slug = @"[a-z0-9]*"
                             }
            );

            routes.MapLowercaseRoute(
                name: "Show",
                url: "{slug}/{version}",
                defaults: new { controller = "Home", action = "Show", version = UrlParameter.Optional },
                constraints: new {
                                 httpMethod = new HttpMethodConstraint("GET"),
                                 slug = @"[a-z0-9]+",
                                 version = @"\d*"
                             }
            );

            routes.MapLowercaseRoute(
                name: "Latest",
                url: "{slug}/latest",
                defaults: new { controller = "Home", action = "Latest" },
                constraints: new {
                                 httpMethod = new HttpMethodConstraint("GET"),
                                 slug = @"[a-z0-9]+"
                             }
            );
        }

        private static void RegisterBundles(BundleCollection bundles) {
            var css = new Bundle("~/css");
            css.AddFile("~/assets/css/vendor/bootstrap-2.0.2.css");
            css.AddFile("~/assets/css/vendor/codemirror-2.23.css");
            css.AddFile("~/assets/css/vendor/codemirror-neat-2.23.css");
            css.AddFile("~/assets/css/compilify.css");
            bundles.Add(css);

            var js = new Bundle("~/js");
            js.AddFile("~/assets/js/vendor/json2.js");
            js.AddFile("~/assets/js/vendor/underscore-1.3.1.js");
            js.AddFile("~/assets/js/vendor/backbone-0.9.2.js");
            js.AddFile("~/assets/js/vendor/bootstrap-2.0.2.js");
            js.AddFile("~/assets/js/vendor/codemirror-2.23.js");
            js.AddFile("~/assets/js/vendor/codemirror-clike-2.23.js");
            js.AddFile("~/assets/js/vendor/jquery.signalr.js");
            js.AddFile("~/assets/js/compilify.js");
            bundles.Add(js);

            if (!HttpContext.Current.IsDebuggingEnabled) {
                css.Transform = new CssMinify();
                js.Transform = new JsMinify();
            }
        }
    }
}
