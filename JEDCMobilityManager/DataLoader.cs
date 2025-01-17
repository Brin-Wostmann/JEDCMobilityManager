using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Text;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class DataLoader
    {
        private static string RootPath = @"D:\JEDC Data\quadrant-io-mobility-data";
        private static string InsertCommand = @$"
INSERT INTO [dbo].[Point]
SELECT @date AS [Date], *
FROM OPENJSON(@json) WITH (
	[DeviceId] VARCHAR(MAX) '$.device_id',
	[IdType] VARCHAR(MAX) '$.id_type',
	[Latitude] DECIMAL(8,4) '$.latitude',
	[Longitude] DECIMAL(8,4) '$.longitude',
	[HorizontalAccuracy] DECIMAL(4,2) '$.horizontal_accuracy',
	[IpAddress] VARCHAR(16) '$.ip_address',
	[DeviceOS] VARCHAR(MAX) '$.device_os',
	[OSVersion] VARCHAR(4) '$.os_version',
	[UserAgent] VARCHAR(MAX) '$.user_agent',
	[Country] VARCHAR(2) '$.country',
	[SourceId] VARCHAR(MAX) '$.source_id',
	[PublisherId] VARCHAR(MAX) '$.publisher_id',
	[AppId] VARCHAR(MAX) '$.app_id',
	[LocationContext] VARCHAR(MAX) '$.location_context',
	[Geohash] VARCHAR(MAX) '$.geohash',
	[Consent] VARCHAR(4) '$.consent',
	[QuadId] VARCHAR(MAX) '$.quad_id',
	[Time] BIGINT '$.timestamp'
)";

        public void Start()
        {
            using (var sql = new SqlConnection(new SqlConnectionStringBuilder {
                DataSource = "localhost",
                InitialCatalog = "JEDCMobility",
                IntegratedSecurity = true,
                TrustServerCertificate = true
            }.ToString()))
            using (var sqlCmd = new SqlCommand(InsertCommand, sql))
            {
                var jsonParam = sqlCmd.Parameters.Add("@json", SqlDbType.NVarChar);
                var dateParam = sqlCmd.Parameters.Add("@date", SqlDbType.Date);
                sql.Open();

                foreach (var file in GetFiles(RootPath).Take(1))
                {
                    var builder = new StringBuilder("[")
                        .AppendJoin(',', ReadLines(file.Item2).Take(1))
                        .Append(']');
                    jsonParam.Value = builder.ToString();
                    dateParam.Value = file.Item1;
                    sqlCmd.ExecuteNonQuery();
                }
                sql.Close();
            }
        }

        private IEnumerable<Tuple<DateOnly, FileInfo>> GetFiles(string root)
        {
            foreach (var yearDir in new DirectoryInfo(root).EnumerateDirectories())
            {
                var year = int.Parse(yearDir.Name.Split('=').Last());
                foreach (var monthDir in yearDir.EnumerateDirectories())
                {
                    var month = int.Parse(monthDir.Name.Split('=').Last());
                    foreach (var dayDir in monthDir.EnumerateDirectories())
                    {
                        var day = int.Parse(dayDir.Name.Split('=').Last());
                        var date = new DateOnly(year, month, day);
                        foreach (var file in dayDir.EnumerateFiles())
                            yield return Tuple.Create(date, file);
                    }
                }
            }
        }

        private IEnumerable<string> ReadLines(FileInfo file)
        {
            using (var fs = file.OpenRead())
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            using (var reader = new StreamReader(gz))
            {
                var line = reader.ReadLine();
                while (line != null)
                    yield return line;
            }
        }
    }
}
