using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Models;
using Compilify.Services;
using Compilify.Web.Models;
using Compilify.Web.Services;

namespace Compilify.Web.Controllers
{
    public class HomeController : AsyncController
    {
        public HomeController(IPostRepository contentRepository)
        {
            db = contentRepository;
        }

        private readonly IPostRepository db;

        public ActionResult Index()
        {
            var viewModel = new PostViewModel();

            const string code = "return \"Hello, world!\";";
            var compiler = new CSharpCompiler();

            viewModel.Post.Content = code;

            viewModel.Errors = compiler.GetCompilationErrors(code);
            return View("Show", viewModel);
        }

        // GET /:slug           -> Equivilant to /:slug/1
        // GET /:slug/:version  -> Get a specific version of the content
        // GET /:slug/latest    -> Get the latest saved version of the content
        // GET /:slug/live      -> Watch or collaborate on the content in real time
        //
        [HttpGet]
        public ActionResult Show(string slug, string version)
        {
            int versionNumber;
            Post content = null;
            if (string.IsNullOrEmpty(version))
            {
                content = db.GetVersion(slug, 1);
            }
            else if (string.Equals("latest", version, StringComparison.OrdinalIgnoreCase))
            {
                content = db.GetLatestVersion(slug);
            }
            else if (Int32.TryParse(version, out versionNumber))
            {
                content = db.GetVersion(slug, versionNumber);
            }

            if (content == null)
            {
                return HttpNotFound();
            }

            var compiler = new CSharpCompiler();
            var viewModel = new PostViewModel
                            {
                                Post = content, 
                                Errors = compiler.GetCompilationErrors(content.Content)
                            };

            if (Request.IsAjaxRequest())
            {
                return Json(new { status = "ok", data = viewModel }, JsonRequestBehavior.AllowGet);
            }
            
            return View("Show", viewModel);
        }

        [HttpPost]
        public ActionResult Save(string slug, Post post) {
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
        public ActionResult Validate(string code)
        {
            var compiler = new CSharpCompiler();

            var errors = compiler.GetCompilationErrors(code)
                                 .ToArray();

            return Json(new { status = "ok", data = errors });
        }
    }
}
