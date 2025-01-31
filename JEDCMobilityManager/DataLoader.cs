using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class DataLoader
    {
        private static string RootPath = @"D:\JEDC Data\quadrant-io-mobility-data";
        private static string InsertCommand = @$"
INSERT INTO [dbo].[Point] ([TimeStamp],                            [DeviceId], [IdType], [Latitude], [Longitude], [HorizontalAccuracy], [IpAddress], [DeviceOS], [OSVersion], [UserAgent], [Country], [SourceId], [PublisherId], [AppId], [LocationContext], [Geohash], [Consent], [QuadId])
SELECT DATEADD(SECOND, [TimeStamp] / 1000, '1970-01-01 00:00:00'), [DeviceId], [IdType], [Latitude], [Longitude], [HorizontalAccuracy], [IpAddress], [DeviceOS], [OSVersion], [UserAgent], [Country], [SourceId], [PublisherId], [AppId], [LocationContext], [Geohash], [Consent], [QuadId]
FROM OPENJSON(@json) WITH (
	[TimeStamp] BIGINT '$.timestamp',
	[DeviceId] VARCHAR(MAX) '$.device_id',
	[IdType] VARCHAR(MAX) '$.id_type',
	[Latitude] DECIMAL(8,5)  '$.latitude',
	[Longitude] DECIMAL(8,5)  '$.longitude',
	[HorizontalAccuracy] VARCHAR(MAX)  '$.horizontal_accuracy',
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
	[QuadId] VARCHAR(MAX) '$.quad_id'
)";

        public void Start()
        {
            using (var sql = new SqlConnection(new SqlConnectionStringBuilder {
                DataSource = "localhost",
                InitialCatalog = "JEDCMobility",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 0,
                CommandTimeout = 0
            }.ToString()))
            using (var sqlCmd = new SqlCommand(InsertCommand, sql))
            {
                var jsonParam = sqlCmd.Parameters.Add("@json", SqlDbType.NVarChar);
                sql.Open();

                foreach (var chunk in ReadLines(GetFiles(RootPath)).Partition(1000000))
                {
                    var builder = new StringBuilder("[")
                        .AppendJoin(',', chunk)
                        .Append(']');
                    jsonParam.Value = builder.ToString();
                    sqlCmd.ExecuteNonQuery();
                }
                sql.Close();
            }
        }

        private void CheckTimestamp(string json)
        {
            using (var jdoc = JsonDocument.Parse(json))
            {
                var timestamp = jdoc.RootElement.GetProperty("timestamp");

                var strVal = timestamp.ToString();
                if (!timestamp.TryGetInt64(out var value))
                {
                }
            }
        }

        private IEnumerable<FileInfo> GetFiles(string root)
        {
            foreach (var yearDir in new DirectoryInfo(root).EnumerateDirectories())
            foreach (var monthDir in yearDir.EnumerateDirectories())
            foreach (var dayDir in monthDir.EnumerateDirectories())
            foreach (var file in dayDir.EnumerateFiles())
                    yield return file;
        }

        private IEnumerable<string> ReadLines(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            foreach (var line in ReadLines(file))
                yield return line;
        }

        private IEnumerable<string> ReadLines(FileInfo file)
        {
            using (var fs = file.OpenRead())
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            using (var reader = new StreamReader(gz))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }
    }

    internal static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            var enumerator = source.GetEnumerator();
            var more = true;
            do
            {
                yield return Inner();
            } while (more);

            IEnumerable<T> Inner()
            {
                for (var i = 0; i < size; i++)
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    else
                    {
                        more = false;
                        break;
                    }
                }
            }
        }
    }
}
