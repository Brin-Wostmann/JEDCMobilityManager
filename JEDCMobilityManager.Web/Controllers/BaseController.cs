using Microsoft.AspNetCore.Mvc;

namespace JEDCMobilityManager.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public static string Connection { get; set; } = "";
    }
}
