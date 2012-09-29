using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Compilify.Web.Commands;
using Compilify.Web.Models;
using Compilify.Web.Queries;

namespace Compilify.Web.Controllers
{
    public class HomeController : CompilifyController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await Resolve<SamplePostQuery>().Execute();

            return View("Show", model);
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Show(string slug, int? version)
        {
            if (version <= 1)
            {
                // Redirect the user to /:slug instead of /:slug/1
                return RedirectToActionPermanent("Show", "Home", new { slug, version = (int?)null });
            }

            var result = await Resolve<PostByIdAndVersionQuery>().Execute(slug, version ?? 1);

            if (result == null)
            {
                return PostNotFound();
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { status = "ok", data = result }, JsonRequestBehavior.AllowGet);
            }
            
            return View("Show", result);
        }

        [HttpGet]
        public async Task<ActionResult> Latest(string slug)
        {
            var version = await Resolve<LastestVersionOfPostQuery>().Execute(slug);

            if (version < 1)
            {
                return PostNotFound();
            }

            // There should only be one URL to represent a given resource, this is just a shortcut for the 
            // user to locate the URL representing the latest version of a given post
            return RedirectToAction("Show", "Home", BuildRouteParametersForPost(slug, version));
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Save(string slug, PostViewModel postViewModel)
        {
            var result = await Resolve<SavePostCommand>().Execute(slug, postViewModel);

            return RedirectToAction("Show", BuildRouteParametersForPost(result.Slug, result.Version));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Validate(PostViewModel postViewModel)
        {
            var errors = Resolve<ErrorsInPostQuery>().Execute(postViewModel.ToPost());
            return Json(new { status = "ok", data = errors });
        }

        private static RouteValueDictionary BuildRouteParametersForPost(string slug, int? version)
        {
            var routeValues = new RouteValueDictionary
                              {
                                  { "slug", slug }
                              };

            if (version.HasValue && version > 1)
            {
                routeValues.Add("version", version);
            }

            return routeValues;
        }

        private ActionResult PostNotFound()
        {
            Response.StatusCode = 404;

            // TODO: Remove dependency on ViewBag
            ViewBag.Message = string.Format("sorry, we couldn't find that...");
            return View("Error");
        }
    }
}
