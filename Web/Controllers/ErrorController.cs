using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Compilify.Web.Controllers
{
    public class ErrorController : Controller
    {
        private static readonly ISet<int> StatusCodes =
            new HashSet<int>(Enum.GetValues(typeof(HttpStatusCode)).Cast<int>().Where(x => x >= 400));

        public ActionResult Index(int status = 500)
        {
            var httpCode = StatusCodes.FirstOrDefault(c => c == status);
            if (httpCode == default(int))
            {
                httpCode = 500;
            }

            // ensure response is the right code
            Response.StatusCode = httpCode;

            switch (httpCode)
            {
                case 404:
                    ViewBag.Message = "The page you were looking for could not be found.";
                    break;
                default:
                    ViewBag.Message = "An error occurred while processing your request.";
                    break;
            }
            
            return View("Error");
        }
    }
}
