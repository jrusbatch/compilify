using System.Web.Routing;

namespace Compilify.Web.Infrastructure
{
    public class LowercaseRoute : Route
    {
        public LowercaseRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler) { }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var path = base.GetVirtualPath(requestContext, values);

            if (path != null)
            {
                path.VirtualPath = path.VirtualPath.ToLowerInvariant();
            }

            return path;
        }
    }
}