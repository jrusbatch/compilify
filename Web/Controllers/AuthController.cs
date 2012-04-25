using System;
using System.Web;
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
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, passwordQuestion: null, 
                                      passwordAnswer: null, isApproved: true, providerUserKey: null, 
                                      status: out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return Redirect(FormsAuthentication.DefaultUrl);
                }
                
                ModelState.AddModelError("", ErrorCodeToString(createStatus));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

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

            request.AddExtension(new ClaimsRequest
                                 {
                                     Email = DemandLevel.Require,
                                     Nickname = DemandLevel.Require
                                 });

            return request.RedirectingResponse.AsActionResult();
        }

        [HttpGet]
        public ActionResult OAuth(string returnUrl)
        {
            var response = openId.GetResponse();

            if (response == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

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

                    var user = Membership.GetUser(username) ??
                               Membership.CreateUser(username, Guid.NewGuid().ToString("N"), email);
                    var userId = ((Guid)user.ProviderUserKey).ToString("N");
                    var cookie = CreateAuthenticationCookie(userId);

                    Response.Cookies.Add(cookie);

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

            return RedirectToAction("SignIn", "Auth");
        }

        private HttpCookie CreateAuthenticationCookie(string userId)
        {
            var issued = DateTime.Now;
            var expires = issued + FormsAuthentication.Timeout;
            var ticket = new FormsAuthenticationTicket(1, userId, issued, expires, true, userId);

            return new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
                   {
                       Domain = FormsAuthentication.CookieDomain,
                       Path = FormsAuthentication.FormsCookiePath,
                       Secure = FormsAuthentication.RequireSSL,
                       Expires = ticket.Expiration,
                       HttpOnly = true,
                   };
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}
