using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Compilify.Web.Services;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public ActionResult Index()
        {
            var builder = new StringBuilder();

            builder.AppendLine("string Greet()");
            builder.AppendLine("{");
            builder.AppendLine("    return \"Hello, world!\";");
            builder.AppendLine("}");
            builder.AppendLine("");
            builder.AppendLine("Greet();");

            var compiler = new CodeExecuter();
            var code = builder.ToString();

            ViewBag.Define = code;
            ViewBag.Observe = compiler.Execute(code);

            return View();
        }

        public ActionResult Validate(string code)
        {

            var tree = SyntaxTree.ParseCompilationUnit(code);

            var result = tree.GetDiagnostics()
                .Select(x => new
                             {
                                 Start = x.Location.SourceSpan.Start,
                                 End = x.Location.GetLineSpan(false),
                                 Message = x.Info.GetMessage(CultureInfo.InvariantCulture)
                             })
                .ToArray();


            return Json(new { status = "ok", data = result });
        }
    }
}
