using System.Text;
using System.Web.Mvc;
using Compilify.Web.Services;
using Roslyn.Compilers;

namespace Compilify.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var compiler = new ScriptExecuter();
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
            var compiler = new ScriptExecuter();

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
