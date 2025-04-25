using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public static string Connection { get; set; } = "";

        protected SqlConnection CreateDataContext()
        {
            return new SqlConnection(Connection);
        }
    }
}
