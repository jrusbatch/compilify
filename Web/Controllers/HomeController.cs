using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Models;
using Compilify.Services;
using Compilify.Web.Infrastructure.Extensions;
using Compilify.Web.Models;
using Compilify.Web.Services;
using Roslyn.Compilers;

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
            var viewModel = new PostViewModel
                            {
                                Errors = Enumerable.Empty<EditorError>()
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

            version = version ?? 1;
            var post = db.GetVersion(slug, version.Value);

            if (post == null)
            {
                return PostNotFound();
            }

            var errors = compiler.GetCompilationErrors(post.Content, post.Classes)
                                 .Select(x => new EditorError
                                              {
                                                  Location = x.Location.GetLineSpan(true),
                                                  Message = x.Info.GetMessage()
                                              });

            var viewModel = new PostViewModel
                            {
                                Errors = errors
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
                return PostNotFound();
            }

            return RedirectToAction("Show", "Home", new { slug = slug, version = latest });
        }

        [HttpPost]
        public ActionResult Save(string slug, Post post)
        {
            if (Request.IsAuthenticated)
            {
                var userId = User.Identity.ToCompilifyIdentity().UserId;
                if (post.AuthorId != userId)
                {
                    slug = null;
                }

                post.AuthorId = userId;
            }

            var result = db.Save(slug, post);

            var routeValues = new RouteValueDictionary { { "slug", result.Slug } };

            if (result.Version > 1)
            {
                routeValues.Add("version", result.Version);
            }

            return RedirectToAction("Show", routeValues);
        }

        [HttpPost]
        public ActionResult Validate(ValidateViewModel viewModel)
        {
            var errors = compiler.GetCompilationErrors(viewModel.Command, viewModel.Classes)
                                 .Where(x => x.Info.Severity > DiagnosticSeverity.Warning)
                                 .Select(x => new EditorError
                                              {
                                                  Location = x.Location.GetLineSpan(true),
                                                  Message = x.Info.GetMessage()
                                              });

            return Json(new { status = "ok", data = errors });
        }

        private ActionResult PostNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Message = string.Format("sorry, we couldn't find that...");
            return View("Error");
        }
    }
}
