using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            compiler = new CSharpValidator(new CSharpCompilationProvider());
        }

        private readonly IPostRepository db;
        private readonly CSharpValidator compiler;
        
        [HttpGet]
        public ActionResult Index()
        {
            var post = BuildSamplePost();

            var errors = GetErrorsInPost(post);

            var viewModel = new PostViewModel(post)
                            {
                                Errors = errors
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
            
            var errors = GetErrorsInPost(post);

            var viewModel = new PostViewModel(post)
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
            var version = db.GetLatestVersion(slug);

            if (version < 1)
            {
                return PostNotFound();
            }

            return RedirectToAction("Show", "Home", BuildRouteParametersForPost(slug, version));
        }

        [HttpPost]
        [ValidateInput(false)]
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

            return RedirectToAction("Show", BuildRouteParametersForPost(result.Slug, result.Version));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Validate(ValidateViewModel viewModel)
        {
            var post = new Post { Classes = viewModel.Classes, Content = viewModel.Command };

            var errors = GetErrorsInPost(post);

            return Json(new { status = "ok", data = errors });
        }

        private IEnumerable<EditorError> GetErrorsInPost(Post post)
        {
            return compiler.GetCompilationErrors(post)
                           .Select(x => new EditorError
                                        {
                                            Location = x.Location.GetLineSpan(true),
                                            Message = x.Info.GetMessage()
                                        });
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

            post.Classes = builder.AppendLine("class Person")
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
                                  .AppendLine("}")
                                  .ToString();

            post.Content = builder.Clear()
                                  .AppendLine("var person = new Person(name: null);")
                                  .AppendLine("")
                                  .AppendLine("return person.Greet();")
                                  .ToString();
            

            

            return post;
        }

        private ActionResult PostNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Message = string.Format("sorry, we couldn't find that...");
            return View("Error");
        }
    }
}
