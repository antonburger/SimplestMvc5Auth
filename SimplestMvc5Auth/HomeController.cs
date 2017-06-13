using System.Web.Mvc;

namespace SimplestMvc5Auth
{
    [RequireHttps]
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("secured")]
        [Authorize]
        public ActionResult Secured()
        {
            return View();
        }

        [Route("unsecured")]
        public ActionResult Unsecured()
        {
            return View();
        }
    }
}
