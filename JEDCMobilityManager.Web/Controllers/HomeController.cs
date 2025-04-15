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

        public string Features(string name)
        {
            return string.Format("{{ \"type\": \"FeatureCollection\", \"features\": [{0}] }}", string.Join(',', Area.GetAll(Connection, FeatureQuery).Select(feature => $@"{{
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
            SELECT A.[Id], A.[Name], A.[Shape].STAsText(), V.[0] AS [Visitors], V.[1] AS [Residents]
            FROM (
	            SELECT PA.[AreaId], P.[IsResident], COUNT(*) AS [Count]
	            FROM [dbo].[PersonArea] PA
	            JOIN [dbo].[Person] P ON P.[Id] = PA.[PersonId]
	            GROUP BY PA.[AreaId], P.[IsResident]
            ) B
            PIVOT (
	            SUM(B.[Count])
	            FOR B.[IsResident] IN ([0], [1])
            ) V
            RIGHT JOIN [dbo].[Area] A ON A.[Id] = V.[AreaId]";
    }
}
