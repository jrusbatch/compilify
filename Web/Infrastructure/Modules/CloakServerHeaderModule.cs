using System;
using System.Web;

namespace Compilify.Web.Infrastructure.Modules
{
    public class CloakServerHeaderModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context != null && context.Response != null && context.Response.Headers != null)
            {
                context.Response.Headers.Remove("Server");
            }
        }

        public void Dispose() { }
    }
}