﻿using System.Data;
using System.Text;
using JEDCMobilityManager.Utility;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    // See datatypes: https://docs.quadrant.io/quadrant-data-dictionary

    internal class DataLoader : SqlScript
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

        public override void Start(string connection)
        {
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();
                ExecuteCommand(sql, HeaderCommand);
                ImportData(sql);
                ExecuteCommand(sql, FooterCommand);
                sql.Close();
            }
        }

        private void ExecuteCommand(SqlConnection sql, string command)
        {
            using (var cmd = new SqlCommand(command, sql))
                cmd.ExecuteNonQuery();
        }

        private void ImportData(SqlConnection sql)
        {
            using (var sqlCmd = new SqlCommand(BodyCommand, sql))
            {
                var jsonParam = sqlCmd.Parameters.Add("@json", SqlDbType.NVarChar);
                Task? cmdTask = null;
                foreach (var chunk in GetFiles(DataRoot).ReadLines().Partition(100000))
                {
                    var builder = new StringBuilder("[")
                        .AppendJoin(',', chunk)
                        .Append(']');
                    jsonParam.Value = builder.ToString();
                    cmdTask?.Wait();
                    cmdTask = sqlCmd.ExecuteNonQueryAsync();
                }
                cmdTask?.Wait();
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
