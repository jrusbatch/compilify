using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BookSleeve;
using Compilify.Web.Services;

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

            var compiler = new CSharpCompiler();
            var code = builder.ToString();

            ViewBag.Define = code;
            ViewBag.Observe = compiler.GetCompilationErrors(code);

            return View();
        }

        [HttpPost]
        public ActionResult Compile(string code)
        {
            return Json(new { status = "ok" });
        }

        [HttpPost]
        public ActionResult Validate(string code)
        {
            var compiler = new CSharpCompiler();

            var errors = compiler.GetCompilationErrors(code)
                                 .Select(x => new
                                              {
                                                  Severity = x.Info.Severity,
                                                  Location = x.Location.GetLineSpan(false),
                                                  Message = x.Info.GetMessage(CultureInfo.InvariantCulture)
                                              })
                                 .ToArray();

            return Json(new { status = "ok", data = errors });

            //var tree = SyntaxTree.ParseCompilationUnit(code);

            //var result = tree.GetDiagnostics()
            //    .Select(x => new
            //                 {
            //                     Severity = x.Info.Severity,
            //                     Location = x.Location.GetLineSpan(false),
            //                     Message = x.Info.GetMessage(CultureInfo.InvariantCulture)
            //                 })
            //    .ToArray();
        }
    }
}
