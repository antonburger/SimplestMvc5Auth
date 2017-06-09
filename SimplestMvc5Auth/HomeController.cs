using System.Web.Mvc;

namespace SimplestMvc5Auth
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("secured")]
        [Authorize]
        public ActionResult Secured()
        {
            return View();
        }

        [HttpGet]
        [Route("unsecured")]
        public ActionResult Unsecured()
        {
            return View();
        }
    }
}
