using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Compilify.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index(int? status) 
        {
            var codes = Enum.GetValues(typeof (HttpStatusCode)).Cast<int>();

            var httpCode = status.HasValue
                ? (int?)codes.FirstOrDefault(c => c == status.Value) ?? 500
                : 500;

            // insure response is the right code
            Response.StatusCode = httpCode;

            switch (httpCode) {
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
