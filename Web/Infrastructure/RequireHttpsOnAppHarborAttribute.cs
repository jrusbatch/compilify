using System;
using System.Web.Mvc;

namespace Compilify.Web.Infrastructure
{
    /// <summary>
    /// Redirects HTTP requests to HTTPS.</summary>
    /// <remarks>
    /// Overriding the default behavior is necessary to avoid a redirect loop. See this article for details:
    /// http://support.appharbor.com/kb/tips-and-tricks/ssl-and-certificates </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RequireHttpsOnAppHarborAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (filterContext.HttpContext.Request.IsSecureConnection)
            {
                return;
            }

            var header = filterContext.HttpContext.Request.Headers["X-Forwarded-Proto"];
            if (string.Equals(header, "https", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            HandleNonHttpsRequest(filterContext);
        }
    }
}
