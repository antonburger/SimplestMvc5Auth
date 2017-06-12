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
    public class AuthenticationController : Controller
    {
        private IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
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
                var userManager = HttpContext.GetOwinContext().GetUserManager<UserManager>();
                var user = await input.AuthenticateAsync(userManager);
                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
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
                    var userManager = HttpContext.GetOwinContext().GetUserManager<UserManager>();
                    var existingUser = await userManager.FindByNameAsync(input.UserName);
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
                        var result = await userManager.CreateAsync(user);
                        await userManager.AddPasswordAsync(user.Id, input.Password);
                    }
                }
            }

            return View("register", input);
        }

        [Route("logout", Name = "Logout")]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("login");
        }
    }
}
