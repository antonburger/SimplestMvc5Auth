using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimplestMvc5Auth
{
    public class AuthenticationController : Controller
    {
        private IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        [HttpGet]
        [Route("login")]
        public ActionResult Show(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("login", Name = "Login")]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel input, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (input.HasValidUsernameAndPassword)
                {
                    var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, input.Username),
                        },
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.Name, ClaimTypes.Role);

                    identity.AddClaim(new Claim(ClaimTypes.Role, "guest"));

                    Authentication.SignIn(new AuthenticationProperties
                        {
                            IsPersistent = input.RememberMe,
                        },
                        identity);

                    return string.IsNullOrEmpty(returnUrl)
                        ? (ActionResult)RedirectToAction("Index", "Home")
                        : Redirect(returnUrl);
                }
            }

            return View("show", input);
        }

        [HttpGet]
        [Route("logout", Name = "Logout")]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login");
        }
    }
}
