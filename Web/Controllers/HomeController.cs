using System;
using System.Text;
using System.Web.Mvc;
using Compilify.Web.Services;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public ActionResult Index()
        {
            var compiler = new CodeExecuter();
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
    }
}
