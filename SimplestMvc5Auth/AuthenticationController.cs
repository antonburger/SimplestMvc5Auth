using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SimplestMvc5Auth.Identity;

namespace SimplestMvc5Auth
{
    [RequireHttps]
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private UserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<UserManager>(); }
        }

        [Route("login")]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel input, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await input.AuthenticateAsync(UserManager);
                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        },
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.Name, ClaimTypes.Role);

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

            return View(input);
        }

        [HttpPost]
        [Route("login-external")]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ConfirmExternalLogin", "Authentication", new { ReturnUrl = returnUrl }));
        }

        [Route("confirm-login-external")]
        public async Task<ActionResult> ConfirmExternalLogin(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var info = await Authentication.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    ModelState.AddModelError("", "Failed to log in via external provider.");
                    return View("Login");
                }

                var user = await UserManager.FindAsync(info.Login);
                if (user == null)
                {
                    ModelState.AddModelError("", "External provider identity is not registered to any users.");
                    return View("Login");
                }

                // TODO: Is this duplication from Login needed?
                var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    },
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name, ClaimTypes.Role);

                Authentication.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false,
                    },
                    identity);

                return string.IsNullOrEmpty(returnUrl)
                    ? (ActionResult)RedirectToAction("Index", "Home")
                    : Redirect(returnUrl);
            }

            return View("Login");
        }

        [Route("register")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel input)
        {
            if (ModelState.IsValid)
            {
                if (input.Password != input.ConfirmPassword)
                {
                    ModelState.AddModelError("PasswordsDontMatch", "The passwords don't match.");
                }
                else
                {
                    var existingUser = await UserManager.FindByNameAsync(input.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserAlreadyExists", "User already exists.");
                    }
                    else
                    {
                        var user = new User(0, input.UserName);
                        // Using the overload of CreateAsync which accepts a password
                        // tries to call SetPasswordHashAsync in the store before the
                        // user is actually created. Bit weird...
                        // Using AddPasswordAsync instead does the user creation first.
                        // Someone else has come across this here:
                        // https://stackoverflow.com/questions/20161598/what-are-all-the-things-setpasswordhashasync-must-accomplish-in-the-ipasswordsto
                        var result = await UserManager.CreateAsync(user);
                        await UserManager.AddPasswordAsync(user.Id, input.Password);
                    }
                }
            }

            return View("register", input);
        }

        [Authorize]
        [Route("associate-external")]
        public ActionResult AssociateExternalLogin(string provider)
        {
            return new ChallengeResult(provider, Url.Action("ConfirmAssociateExternalLogin", "Authentication"), User.Identity.GetUserId());
        }

        [Authorize]
        [Route("confirm-associate-external")]
        public async Task<ActionResult> ConfirmAssociateExternalLogin()
        {
            if (ModelState.IsValid)
            {
                var info = await Authentication.GetExternalLoginInfoAsync("XsrfId", User.Identity.GetUserId());
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), info.Login);
                return View("ExternalLoginSuccess");
            }
            return View("ExternalLoginFailure");
        }

        [Authorize]
        [Route("logout")]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login");
        }
    }
}
