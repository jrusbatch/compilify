using System;
using System.Web.Mvc;
using System.Web.Security;
using Compilify.Web.Models;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace Compilify.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly OpenIdRelyingParty openId = new OpenIdRelyingParty();

        [HttpGet]
        public ActionResult SignIn(string returnUrl)
        {
            return View();
        }
        
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult OAuth(SignInViewModel model, string returnUrl)
        {
            if (!Identifier.IsValid(model.OpenId))
            {
                ModelState.AddModelError("OpenId", "The specified login identifier is invalid");
                return View();
            }

            var request = openId.CreateRequest(Identifier.Parse(model.OpenId));

            request.AddExtension(new ClaimsRequest { Email = DemandLevel.Require });

            return request.RedirectingResponse.AsActionResult();
        }

        [HttpGet]
        public ActionResult OAuth(string returnUrl)
        {
            var response = openId.GetResponse();

            if (response != null)
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                    {
                        var username = response.ClaimedIdentifier;
                        string email = null;

                        var fields = response.GetExtension<ClaimsResponse>();
                        if (fields != null)
                        {
                            email = fields.Email;
                        }

                        if (Membership.GetUser(username) == null)
                        {
                            Membership.CreateUser(username, Guid.NewGuid().ToString("N"), email);
                        }

                        FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
                        return Redirect(returnUrl ?? FormsAuthentication.DefaultUrl);
                    }
                    case AuthenticationStatus.Canceled:
                    {
                        ModelState.AddModelError("loginIdentifier", "Login was cancelled at the provider");
                        break;
                    }
                    case AuthenticationStatus.Failed:
                    {
                        ModelState.AddModelError("loginIdentifier",  "Login failed using the provided OpenID identifier");
                        break;
                    }
                }
            }

            return RedirectToAction("SignIn", "Auth");
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
