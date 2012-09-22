using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.LanguageServices;
using Compilify.Models;
using Compilify.Web.Models;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        private readonly IPostRepository db;
        private readonly ICodeValidator compiler;
        
        public HomeController(IPostRepository contentRepository, ICodeValidator codeValidator)
        {
            if (contentRepository == null)
            {
                throw new ArgumentNullException("contentRepository");
            }
            
            if (codeValidator == null)
            {
                throw new ArgumentNullException("codeValidator");
            }

            db = contentRepository;
            compiler = codeValidator;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var post = BuildSamplePost();

            var errors = GetErrorsInPost(post);

            var viewModel = PostViewModel.Create(post);

            viewModel.Errors = errors;

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
        [HttpGet]
        public ActionResult Show(string slug, int? version)
        {
            if (version <= 1)
            {
                // Redirect the user to /:slug instead of /:slug/1
                return RedirectToActionPermanent("Show", "Home", new { slug, version = (int?)null });
            }

            version = version ?? 1;
            var post = db.GetVersion(slug, version.Value);

            if (post == null)
            {
                return PostNotFound();
            }
            
            var errors = GetErrorsInPost(post);

            var viewModel = PostViewModel.Create(post);

            viewModel.Errors = errors;

            if (Request.IsAjaxRequest())
            {
                return Json(new { status = "ok", data = viewModel }, JsonRequestBehavior.AllowGet);
            }
            
            return View("Show", viewModel);
        }

        [HttpGet]
        public ActionResult Latest(string slug)
        {
            var version = db.GetLatestVersion(slug);

            if (version < 1)
            {
                return PostNotFound();
            }

            return RedirectToAction("Show", "Home", BuildRouteParametersForPost(slug, version));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(string slug, PostViewModel postViewModel)
        {
            var result = db.Save(slug, postViewModel.ToPost());

            return RedirectToAction("Show", BuildRouteParametersForPost(result.Slug, result.Version));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Validate(PostViewModel postViewModel)
        {
            var errors = GetErrorsInPost(postViewModel.ToPost());

            return Json(new { status = "ok", data = errors });
        }

        private static RouteValueDictionary BuildRouteParametersForPost(string slug, int? version)
        {
            var routeValues = new RouteValueDictionary { { "slug", slug } };

            if (version.HasValue && version > 1)
            {
                routeValues.Add("version", version);
            }

            return routeValues;
        }

        private static Post BuildSamplePost()
        {
            var post = new Post();
            var builder = new StringBuilder();

            builder.AppendLine("class Person")
                   .AppendLine("{")
                   .AppendLine("    public Person(string name)")
                   .AppendLine("    {")
                   .AppendLine("        Name = name;")
                   .AppendLine("    }")
                   .AppendLine()
                   .AppendLine("    public string Name { get; private set; }")
                   .AppendLine()
                   .AppendLine("    public string Greet()")
                   .AppendLine("    {")
                   .AppendLine("        if (Name == null)")
                   .AppendLine("            return \"Hello, stranger!\";")
                   .AppendLine()
                   .AppendLine("        return string.Format(\"Hello, {0}!\", Name);")
                   .AppendLine("    }")
                   .AppendLine("}");

            post.AddDocument("Classes", builder.ToString());

            builder.Clear()
                   .AppendLine("var person = new Person(name: null);")
                   .AppendLine()
                   .AppendLine("return person.Greet();");

            post.AddDocument("Content", builder.ToString());

            return post;
        }

        private IEnumerable<EditorError> GetErrorsInPost(ICodeProject post)
        {
            return compiler.GetCompilationErrors(post);
        }

        private ActionResult PostNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Message = string.Format("sorry, we couldn't find that...");
            return View("Error");
        }
    }
}
