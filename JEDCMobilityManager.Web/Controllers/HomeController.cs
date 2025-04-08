using JEDCMobilityManager.Utility;
using Microsoft.AspNetCore.Mvc;

namespace JEDCMobilityManager.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View(Area.GetAll(Connection));
        }
    }
}
