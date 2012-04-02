using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Roslyn.Compilers;
using Roslyn.Scripting.CSharp;

namespace Compilify.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var compiler = new ScriptEngine(new Assembly[0], new[] { "System" });
            var builder = new StringBuilder();

            builder.AppendLine("string Greet()");
            builder.AppendLine("{");
            builder.AppendLine("    return \"Hello, world!\";");
            builder.AppendLine("}");
            builder.AppendLine("");
            builder.AppendLine("Greet();");

            var code = builder.ToString();

            ViewBag.Define = code;
            ViewBag.Observe = compiler.Execute(code);

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
