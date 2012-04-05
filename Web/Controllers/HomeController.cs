using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Compilify.Web.Models;
using Compilify.Web.Services;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public HomeController(ISequenceProvider sequenceProvider, IPageContentRepository contentRepository)
        {
            urlCounter = sequenceProvider;
            db = contentRepository;
        }

        private readonly ISequenceProvider urlCounter;
        private readonly IPageContentRepository db;

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

        [HttpGet]
        public ActionResult Show(string slug, int version = 1)
        {
            var content = db.Get(slug, version);
            if (content == null)
            {
                return HttpNotFound();
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { status = "ok", data = content }, JsonRequestBehavior.AllowGet);
            }
            
            var compiler = new CSharpCompiler();

            ViewBag.Define = content.Code;
            ViewBag.Observe = compiler.GetCompilationErrors(content.Code);

            return View("Index");
        }

        [HttpPost]
        public ActionResult Save(string slug, PageContent content)
        {
            if (slug == null)
            {
                var id = (int) urlCounter.Next();
                slug = Base32Encoder.Encode(id);
                content.Slug = slug;
            }

            var result = db.Save(slug, content);

            var url = result.Version > 1 
                      ? Url.Action("Show", new { slug = result.Slug, version = result.Version }) 
                      : Url.Action("Show", new { slug = result.Slug });

            return Json(new { status = "ok", data = new { slug = result.Slug, version = result.Version, url = url } });
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
        }
    }
}
