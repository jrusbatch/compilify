using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Web.EndPoints;
using Compilify.Web.Infrastructure.Extensions;
using SignalR;

namespace Compilify.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Root",
                string.Empty,
                defaults: new { controller = "Home", action = "Index" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") });

            routes.MapRoute(
                name: "About",
                url: "about",
                defaults: new { controller = "Home", action = "About" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") });

            routes.MapRoute(
                name: "validate",
                url: "validate",
                defaults: new { controller = "Home", action = "Validate" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") });

            routes.MapConnection<ExecuteEndPoint>("execute", "execute/{*operation}");

            routes.MapRoute(
                name: "Update",
                url: "{slug}/{version}",
                defaults: new { controller = "Home", action = "Save", version = UrlParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint("POST"), slug = @"[a-z0-9]*", });

            routes.MapRoute(
                name: "Save",
                url: "{slug}",
                defaults: new { controller = "Home", action = "Save", slug = UrlParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint("POST"), slug = @"[a-z0-9]*" });

            routes.MapRoute(
                name: "Show",
                url: "{slug}/{version}",
                defaults: new { controller = "Home", action = "Show", version = UrlParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint("GET"), slug = @"[a-z0-9]+", version = @"\d*" });

            routes.MapRoute(
                name: "Latest",
                url: "{slug}/latest",
                defaults: new { controller = "Home", action = "Latest" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET"), slug = @"[a-z0-9]+" });

            routes.MapRoute(
                "Error",
                "Error/{status}",
                new { controller = "Error", action = "Index", status = UrlParameter.Optional });

            // 404s
            routes.MapRoute(
                "404", "{*url}", new { controller = "Error", action = "Index", status = 404 });
        }
    }
}