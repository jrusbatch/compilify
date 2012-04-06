using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using BookSleeve;
using Compilify.Web.Models;
using Compilify.Web.Services;
using Microsoft.Web.Optimization;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Compilify.Web
{
    public class Application : HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MvcHandler.DisableMvcResponseHeader = true;

            ConfigureIoC();
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
                defaults: new { controller = "Home", action = "Index" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "validate",
                url: "validate",
                defaults: new { controller = "Home", action = "Validate" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                name: "Save",
                url: "{slug}",
                defaults: new { controller = "Home", action = "Save", slug = UrlParameter.Optional },
                constraints: new
                             {
                                 httpMethod = new HttpMethodConstraint("POST"),
                                 slug = @"[a-z0-9]*"
                             }
            );

            routes.MapRoute(
                name: "Show",
                url: "{slug}/{version}",
                defaults: new { controller = "Home", action = "Show", version = UrlParameter.Optional },
                constraints: new
                             {
                                 httpMethod = new HttpMethodConstraint("GET"),
                                 slug = @"[a-z0-9]+"
                             }
            );

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}",
            //    defaults: new { controller = "Home", action = "Index" }
            //);
        }

        private static void RegisterBundles(BundleCollection bundles)
        {
            var css = new Bundle("~/vendor/css", typeof(CssMinify));
            css.AddDirectory("~/assets/css/vendor", "*.css", false);
            bundles.Add(css);

            var js = new Bundle("~/vendor/js", typeof(JsMinify));
            js.AddDirectory("~/assets/js/vendor", "*.js", false);
            bundles.Add(js);
        }

        private static void ConfigureIoC()
        {
            var assembly = typeof(Application).Assembly;
            var builder = new ContainerBuilder();

            builder.Register(x =>
                             {
                                 var conn = new RedisConnection(ConfigurationManager.AppSettings["REDISTOGO_URL"]);
                                 conn.Wait(conn.Open());
                                 return conn;
                             })
                   .InstancePerHttpRequest()
                   .AsSelf();

            builder.Register(x =>
                             {
                                 var connectionString = ConfigurationManager.AppSettings["MONGOLAB_URI"];
                                 return MongoDatabase.Create(connectionString);
                             })
                   .InstancePerHttpRequest()
                   .AsSelf();

            builder.Register(x => new SequenceProvider(x.Resolve<RedisConnection>()))
                   .AsImplementedInterfaces()
                   .InstancePerHttpRequest();

            builder.RegisterType<PageContentRepository>()
                   .AsImplementedInterfaces()
                   .InstancePerHttpRequest();

            builder.RegisterControllers(assembly);
            builder.RegisterModelBinders(assembly);
            builder.RegisterModelBinderProvider();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterFilterProvider();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
