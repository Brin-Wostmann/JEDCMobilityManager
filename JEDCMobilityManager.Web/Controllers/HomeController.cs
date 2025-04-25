using JEDCMobilityManager.Utility;
using Microsoft.AspNetCore.Mvc;

namespace JEDCMobilityManager.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View(this);
        }

        public string Features(string name)
        {
            const string query = @"
                SELECT A.[Id], A.[Name], A.[Shape].STAsText(), T.[Visitors], T.[Residents]
                FROM [dbo].[Area] A
                LEFT JOIN [dbo].[vw_Total_All] T ON T.[AreaId] = A.[Id]";

            return string.Format("{{ \"type\": \"FeatureCollection\", \"features\": [{0}] }}",
                string.Join(',', Area.GetAll(Connection, query).Select(feature => $@"{{
                    ""type"": ""Feature"",
                    ""properties"": {{
                        ""name"": ""{feature.Name}"",
                        ""visitors"": {feature.OtherValues["Visitors"]},
                        ""residents"": {feature.OtherValues["Residents"]}
                    }},
                    ""geometry"": {feature.GeoJson}
                }}")));
        }

        public JsonResult FeatureStatistics(string id, DateTime? start = null, DateTime? end = null)
        {
            var dateWhere = "";
            if (start.HasValue || end.HasValue)
                dateWhere += "WHERE ";
            if (start.HasValue)
            {
                dateWhere += $"[Date] >= '{start.Value.Date}'";
                if (end.HasValue)
                    dateWhere += " AND ";
            }
            if (end.HasValue)
                dateWhere += $"[Date] < '{end.Value.Date}'";

            return Json(FeatureStatisticQueries.ToDictionary(k => k.Key, v => CreateDataContext().ExecuteQuery(string.Format(v.Value, dateWhere))));
        }

        private static IDictionary<string, string> FeatureStatisticQueries { get; } = new Dictionary<string, string> {
            { "MonthTotals", @"
                SELECT T.[Month], A.[Name], T.[Visitors], T.[Residents]
                FROM [dbo].[Area] A
                LEFT JOIN (
	                SELECT [AreaId], MONTH([Date]) [Month], SUM([Visitors]) [Visitors], SUM([Residents]) [Residents]
	                FROM [dbo].[vw_Total_Daily]
                    {0}
	                GROUP BY [AreaId], MONTH([Date])
                ) T ON T.[AreaId] = A.[Id]
                ORDER BY A.[Id], T.[Month]" },
            { "AvgHourly", @"
                SELECT T.[Hour], A.[Name], T.[AvgVisitors], T.[AvgResidents]
                FROM [dbo].[Area] A
                LEFT JOIN (
	                SELECT [AreaId], [Hour], AVG([Visitors]) [AvgVisitors], AVG([Residents]) [AvgResidents]
	                FROM [dbo].[vw_Total_Hourly]
                    {0}
	                GROUP BY [AreaId], [Hour]
                ) T ON T.[AreaId] = A.[Id]
                ORDER BY A.[Id], T.[Hour]" }
        };
    }
}
