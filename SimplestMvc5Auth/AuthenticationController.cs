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
        [Route("login", Name = "Login")]
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

        [Route("register")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("register", Name = "Register")]
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
            return new ChallengeResult(provider, Url.Action("ConfirmExternalLogin", "Authentication"), User.Identity.GetUserId());
        }

        [Authorize]
        [Route("confirm-external")]
        public async Task<ActionResult> ConfirmExternalLogin()
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

        [Route("logout", Name = "Logout")]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login");
        }
    }
}
