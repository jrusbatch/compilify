using System.Reflection;
using System.Web.Mvc;
using Roslyn.Compilers;
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

            dynamic result;

            try
            {
                result = compiler.Execute(code);
            }
            catch (CompilationErrorException ex)
            {
                return Json(new { data = ex.Message });
            }

            return Json(new { data = result });
        }
    }
}
