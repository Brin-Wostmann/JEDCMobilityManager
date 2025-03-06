using Microsoft.AspNetCore.Mvc;

namespace JEDCMobilityManager.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
