using System.Web.Mvc;

namespace Compilify.Web.Controllers
{
    public class ContentController : AsyncController
    {
        //
        // GET: /Content/

        public ActionResult Index()
        {
            return View();
        }

    }
}
