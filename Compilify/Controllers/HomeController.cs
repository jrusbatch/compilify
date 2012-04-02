using System.Reflection;
using System.Web.Mvc;
using Roslyn.Scripting.CSharp;

namespace Compilify.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Compile(string code)
        {
            var compiler = new ScriptEngine(new Assembly[0], new[] { "System" });

            dynamic result = compiler.Execute(code);

            return Json(new { data = result });
        }
    }
}
