using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Compilify.LanguageServices;
using Compilify.Models;
using Compilify.Web.Commands;
using Compilify.Web.Models;
using Compilify.Web.Queries;
using Raven.Client;

namespace Compilify.Web.Controllers
{
    public class HomeController : BaseMvcController
    {
        private const string ProjectCookieKey = "compilify.currentproject";

        private readonly IDocumentSession session;

        private readonly ICodeValidator validator;

        public HomeController(IDocumentSession documentSession, ICodeValidator codeValidator)
        {
            session = documentSession;
            validator = codeValidator;
        }

        protected virtual string CurrentProjectId
        {
            get
            {
                return Request.Cookies[ProjectCookieKey] != null 
                    ? Request.Cookies[ProjectCookieKey].Value 
                    : string.Empty;
            }
            set
            {
                Response.AppendCookie(new HttpCookie(ProjectCookieKey, value)
                                      {
                                          Expires = DateTime.UtcNow.AddDays(7)
                                      });
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            Project project;

            if (CurrentProjectId.IsEmpty() || (project = session.Load<Project>(CurrentProjectId)) == null)
            {
                project = new Project()
                    .AddOrUpdate(new Document { Name = "Main", Content = "return new Person(\"stranger\").Greet();" })
                    .AddOrUpdate(BuildSampleDocument());

                project.References.Add(new Reference { AssemblyName = "mscorlib", Version = "4.0.0.0" });

                session.Store(project);
                CurrentProjectId = project.Id;
            }

            var viewModel = new PostViewModel()
            {
                Project = project,
                Errors = GetErrorsInProgram(project).ToList()
            };

            return View("Show", viewModel);
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
            var version = await Resolve<LatestVersionOfPostQuery>().Execute(slug);

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

        private static Document BuildSampleDocument()
        {
            var post = new Document { Name = "Person" };

            var builder = new StringBuilder();
            post.Content = builder.AppendLine("public class Person")
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
                                  .AppendLine("        if (string.IsNullOrEmpty(Name))")
                                  .AppendLine("        {")
                                  .AppendLine("            return \"Hello, stranger!\";")
                                  .AppendLine("        }")
                                  .AppendLine()
                                  .AppendLine("        return string.Format(\"Hello, {0}!\", Name);")
                                  .AppendLine("    }")
                                  .AppendLine("}")
                                  .ToString();

            return post;
        }

        private IEnumerable<EditorError> GetErrorsInProgram(ICodeProgram project)
        {
            return validator.GetCompilationErrors(project);
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
