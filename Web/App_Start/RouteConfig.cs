using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Web.EndPoints;
using SignalR;

namespace Compilify.Web
{
    public static class RouteConfig
    {
        private const string ProjectIdentifier = @"[a-z0-9]{32}";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapConnection<ExecuteEndPoint>("execute", "execute/{*operation}");


            routes.MapRoute(
                "Root",
                string.Empty,
                new { controller = "Home", action = "Index" },
                new { httpMethod = new HttpMethodConstraint("GET") });

            routes.MapRoute(
                "About",
                "about",
                new { controller = "Home", action = "About" });

            routes.MapRoute(
                "validate",
                "validate",
                new { controller = "Home", action = "Validate" },
                new { httpMethod = new HttpMethodConstraint("POST") });

            routes.MapRoute(
                "Update",
                "{id}",
                new { controller = "Home", action = "Save" },
                new { httpMethod = new HttpMethodConstraint("POST"), id = ProjectIdentifier });

            routes.MapRoute(
                "Show",
                "{id}",
                new { controller = "Home", action = "Show" },
                new { httpMethod = new HttpMethodConstraint("GET"), id = ProjectIdentifier });

            routes.MapRoute(
                "Error",
                "Error/{status}",
                new { controller = "Error", action = "Index", status = UrlParameter.Optional });

            // 404s
            routes.MapRoute(
                "404",
                "{*url}",
                new { controller = "Error", action = "Index", status = 404 });
        }
    }
}