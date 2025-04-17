using JEDCMobilityManager.Utility;
using Microsoft.AspNetCore.Mvc;

namespace JEDCMobilityManager.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View(Tuple.Create(Features("RD")));
        }

        public string Features(string name)
        {
            return string.Format("{{ \"type\": \"FeatureCollection\", \"features\": [{0}] }}",
                string.Join(',', Area.GetAll(Connection, FeatureQuery).Select(feature => $@"{{
                    ""type"": ""Feature"",
                    ""properties"": {{
                        ""name"": ""{feature.Name}"",
                        ""visitors"": {feature.OtherValues["Visitors"]},
                        ""residents"": {feature.OtherValues["Residents"]}
                    }},
                    ""geometry"": {feature.GeoJson}
                }}")));
        }

        private const string FeatureQuery = @"
            SELECT A.[Id], A.[Name], A.[Shape].STAsText(), T.[Visitors], T.[Residents]
            FROM [dbo].[Area] A
            LEFT JOIN [dbo].[vw_Total_All] T ON T.[AreaId] = A.[Id]";
    }
}
