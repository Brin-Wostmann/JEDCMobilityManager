using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using JEDCMobilityManager.Utility;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class DataLoader
    {
        private string DataRoot { get; set; }
        private string HeaderCommand { get; set; }
        private string BodyCommand { get; set; }
        private string FooterCommand { get; set; }

        public DataLoader(string dataRoot)
        {
            DataRoot = dataRoot;
            HeaderCommand = File.ReadAllText(@"DataLoaderScripts\Header.sql");
            BodyCommand = File.ReadAllText(@"DataLoaderScripts\Body.sql");
            FooterCommand = File.ReadAllText(@"DataLoaderScripts\Footer.sql");
        }

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
            {
                sql.Open();

                using (var sqlCmd = new SqlCommand(BodyCommand, sql))
                {
                    var jsonParam = sqlCmd.Parameters.Add("@json", SqlDbType.NVarChar);
                    Task cmdTask = null;
                    foreach (var chunk in GetFiles(DataRoot).ReadLines().Partition(100000))
                    {
                        var builder = new StringBuilder("[")
                            .AppendJoin(',', chunk)
                            .Append(']');
                        jsonParam.Value = builder.ToString();
                        cmdTask?.Wait();
                        cmdTask = sqlCmd.ExecuteNonQueryAsync();
                    }
                }

                sql.Close();
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
    }
}
