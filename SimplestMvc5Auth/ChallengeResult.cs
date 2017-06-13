using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace SimplestMvc5Auth
{
    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUrl)
            : this(provider, redirectUrl, null)
        {
        }

        public ChallengeResult(string provider, string redirectUrl, string userId)
        {
            Provider = provider;
            RedirectUrl = redirectUrl;
            UserId = userId;
        }

        public string Provider { get; set; }
        public string RedirectUrl { get; set; }
        public string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = RedirectUrl,
            };

            if (UserId != null)
            {
                properties.Dictionary["XsrfId"] = UserId;
            }

            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, Provider);
        }
    }
}
