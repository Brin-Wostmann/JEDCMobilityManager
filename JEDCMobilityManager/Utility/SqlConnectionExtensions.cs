
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager.Utility
{
    public static class SqlConnectionExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ExecuteQuery(this SqlConnection ctx, string cmd)
        {
            ctx.Open();
            using (var c = new SqlCommand(cmd, ctx))
            using (var reader = c.ExecuteReader())
                while (reader.Read())
                    yield return reader.ToDictionary();
            ctx.Close();
        }
    }
}
