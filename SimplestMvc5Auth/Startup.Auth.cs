using System.Configuration;
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

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseGoogleAuthentication(
                clientId: ConfigurationManager.AppSettings["google-oauth-clientid"],
                clientSecret: ConfigurationManager.AppSettings["google-oauth-clientsecret"]);

            app.CreatePerOwinContext(() => new UserManager(new UserStore()));
        }
    }
}
