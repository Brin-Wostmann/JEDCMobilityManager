using JEDCMobilityManager.Web.Controllers;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BaseController.Connection = new SqlConnectionStringBuilder {
                DataSource = "localhost",
                InitialCatalog = "JEDCMobility",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 0,
                CommandTimeout = 0
            }.ToString();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
