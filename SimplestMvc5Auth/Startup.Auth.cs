using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using SimplestMvc5Auth.Identity;

namespace SimplestMvc5Auth
{
    partial class Startup
    {
        public void ConfigureAuthentication(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/login"),
            });

            app.CreatePerOwinContext(() => new UserManager(new UserStore()));
        }
    }
}
