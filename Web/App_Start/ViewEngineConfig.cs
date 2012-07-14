using System.Web.Mvc;

namespace Compilify.Web
{
    public static class ViewEngineConfig
    {
        public static void RegisterViewEngines(ViewEngineCollection viewEngines)
        {
            viewEngines.Clear();
            viewEngines.Add(new RazorViewEngine());
        }
    }
}