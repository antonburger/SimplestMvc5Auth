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
        public ActionResult Show()
        {
            return View();
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel input)
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

                    return RedirectToAction("index", "home");
                }
            }

            return View("show", input);
        }

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login");
        }
    }
}
