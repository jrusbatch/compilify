using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Models;
using Compilify.Services;
using Compilify.Web.Models;
using Compilify.Web.Services;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public HomeController(IPostRepository contentRepository)
        {
            db = contentRepository;
            compiler = new CSharpCompiler();
        }

        private readonly IPostRepository db;
        private readonly CSharpCompiler compiler;

        public ActionResult Index()
        {
            var classesBuilder = new StringBuilder();

            classesBuilder.AppendLine("public interface IPerson");
            classesBuilder.AppendLine("{");
            classesBuilder.AppendLine("    string Name { get; }");
            classesBuilder.AppendLine();
            classesBuilder.AppendLine("    string Greet();");
            classesBuilder.AppendLine("}");
            classesBuilder.AppendLine();
            classesBuilder.AppendLine("class Person : IPerson");
            classesBuilder.AppendLine("{");
            classesBuilder.AppendLine("    public Person(string name)");
            classesBuilder.AppendLine("    {");
            classesBuilder.AppendLine("        Name = name;");
            classesBuilder.AppendLine("    }");
            classesBuilder.AppendLine();
            classesBuilder.AppendLine("    public string Name { get; private set; }");
            classesBuilder.AppendLine();
            classesBuilder.AppendLine("    public string Greet()");
            classesBuilder.AppendLine("    {");
            classesBuilder.AppendLine("        if (Name == null)");
            classesBuilder.AppendLine("            return \"Hello, stranger!\";");
            classesBuilder.AppendLine();
            classesBuilder.AppendLine("        return string.Format(\"Hello, {0}!\", Name);");
            classesBuilder.AppendLine("    }");
            classesBuilder.AppendLine("}");
            
            var commandBuilder = new StringBuilder();

            commandBuilder.AppendLine("IPerson person = new Person(name: null);");
            commandBuilder.AppendLine("");
            commandBuilder.AppendLine("return person.Greet();");
            
            var post = new Post { Content = commandBuilder.ToString(), Classes = classesBuilder.ToString() };

            var viewModel = new PostViewModel
                            {
                                Post = post,
                                Errors = compiler.GetCompilationErrors(post.Content, post.Classes)
                            };

            return View("Show", viewModel);
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        // GET /:slug           -> Equivilant to /:slug/1
        // GET /:slug/:version  -> Get a specific version of the content
        // GET /:slug/latest    -> Get the latest saved version of the content
        // GET /:slug/live      -> Watch or collaborate on the content in real time
        //
        [HttpGet]
        public ActionResult Show(string slug, int? version)
        {
            if (version <= 1)
            {
                // Redirect the user to /:slug instead of /:slug/1
                return RedirectToActionPermanent("Show", "Home", new { slug = slug, version = (int?)null });
            }

            var post = db.GetVersion(slug, version ?? 1);

            if (post == null)
            {
                return HttpNotFound();
            }

            var viewModel = new PostViewModel
                            {
                                Post = post, 
                                Errors = compiler.GetCompilationErrors(post.Content, post.Classes)
                            };

            if (Request.IsAjaxRequest())
            {
                return Json(new { status = "ok", data = viewModel }, JsonRequestBehavior.AllowGet);
            }
            
            return View("Show", viewModel);
        }
        
        [HttpGet]
        public ActionResult Latest(string slug)
        {
            var latest = db.GetLatestVersion(slug);

            if (latest < 1)
            {
                return HttpNotFound();
            }

            return RedirectToAction("Show", "Home", new { slug = slug, version = latest });
        }

        [HttpPost]
        public ActionResult Save(string slug, Post post)
        {
            var result = db.Save(slug, post);

            var routeValues = new RouteValueDictionary { { "slug", result.Slug } };

            if (result.Version > 1)
            {
                routeValues.Add("version", result.Version);
            }

            var url = Url.Action("Show", routeValues);
            return Json(new { status = "ok", data = new { slug = result.Slug, version = result.Version, url = url } });
        }

        [HttpPost]
        public ActionResult Validate(ValidateViewModel viewModel)
        {
            var errors = compiler.GetCompilationErrors(viewModel.Command, viewModel.Classes)
                                 .ToArray();

            return Json(new { status = "ok", data = errors });
        }
    }
}
