using System.Web.Mvc;
using Compilify.Web.Infrastructure;

namespace Compilify.Web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireHttpsOnAppHarborAttribute());
        }
    }
}