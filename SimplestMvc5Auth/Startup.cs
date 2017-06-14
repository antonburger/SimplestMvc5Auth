using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SimplestMvc5Auth.Startup))]

namespace SimplestMvc5Auth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuthentication(app);
        }
    }
}
