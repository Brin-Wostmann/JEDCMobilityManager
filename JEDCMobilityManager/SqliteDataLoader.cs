using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace JEDCMobilityManager
{
    internal class SqliteDataLoader
    {
        private static string RootPath = @"D:\JEDC Data\quadrant-io-mobility-data";
        private const string TableName = "Point";

        private static IDictionary<string, JsonValueKind> PropertyKinds { get; set; }

        static SqliteDataLoader()
        {
            PropertyKinds = JsonDocument.Parse(ReadLines(GetFiles(RootPath)).First()).RootElement.EnumerateObject().ToDictionary(k => k.Name, v => v.Value.ValueKind);
        }

        public void Start()
        {
            using (var sql = new SQLiteConnection(new SQLiteConnectionStringBuilder {
                DataSource = @"D:\SQLite\JEDCMobility.db"
            }.ToString()))
            {
                sql.Open();

                ReadData(sql);

                using (var cmd = new SQLiteCommand(GetCreateCommand(), sql))
                    cmd.ExecuteNonQuery();

                ReadData(sql);

                using (var cmd = new InsertCommand(sql))
                    foreach (var line in ReadLines(GetFiles(RootPath)).Take(10))
                        cmd.Execute(line);

                ReadData(sql);

                sql.Clone();
            }
        }

        private static void ReadData(SQLiteConnection sql)
        {
            using (var cmd = new SQLiteCommand($"SELECT TOP 10 * FROM [{TableName}]", sql))
            using (var reader = cmd.ExecuteReader())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader.GetName(i) + "\t");
                }
                Console.WriteLine();

                while (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write(reader.GetValue(i) + "\t");
                    }
                    Console.WriteLine();
                }
            }
        }

        private static IEnumerable<FileInfo> GetFiles(string root)
        {
            foreach (var yearDir in new DirectoryInfo(root).EnumerateDirectories())
            foreach (var monthDir in yearDir.EnumerateDirectories())
            foreach (var dayDir in monthDir.EnumerateDirectories())
            foreach (var file in dayDir.EnumerateFiles())
                yield return file;
        }

        private static IEnumerable<string> ReadLines(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            foreach (var line in ReadLines(file))
                yield return line;
        }

        private static IEnumerable<string> ReadLines(FileInfo file)
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

        private string GetCreateCommand()
        {
            var jmap = new Dictionary<JsonValueKind, string> {
                { JsonValueKind.String, "TEXT" },
                { JsonValueKind.Number, "REAL" },
                { JsonValueKind.True, "INTEGER" },
                { JsonValueKind.False, "INTEGER" }
            };

            var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS [{TableName}] {{ [Id] INT PRIMARY KEY");
            foreach (var prop in PropertyKinds)
                builder.AppendLine($",[{prop.Key}] {jmap[prop.Value]} NOT NULL");
            return builder.AppendLine("}").ToString();
        }

        private class InsertCommand : IDisposable
        {
            private SQLiteCommand Cmd { get; set; }
            private IDictionary<string, SQLiteParameter> Parameters { get; set; }

            public InsertCommand(SQLiteConnection sql)
            {
                var jmap = new Dictionary<JsonValueKind, DbType> {
                    { JsonValueKind.String, DbType.String },
                    { JsonValueKind.Number, DbType.VarNumeric },
                    { JsonValueKind.True, DbType.Boolean },
                    { JsonValueKind.False, DbType.Boolean }
                };

                Cmd = new SQLiteCommand(GetInsertCommand(), sql);
                Parameters = PropertyKinds.ToDictionary(k => k.Key, v => Cmd.Parameters.Add($@"@{v.Key}", jmap[v.Value]));
            }

            private string GetInsertCommand()
            {
                var tBuilder = new StringBuilder($"INSERT INTO [{TableName}] (");
                var vBuilder = new StringBuilder(" VALUES (");
                foreach (var p in Parameters)
                {
                    tBuilder.Append(p.Key + ", ");
                    vBuilder.Append(p.Value.ParameterName + ", ");
                }
                tBuilder.Length--;
                vBuilder.Length--;
                tBuilder.Append(")");
                vBuilder.Append(")");
                return tBuilder.ToString() + vBuilder.ToString();
            }

            public void Execute(string json)
            {
                SetParameters(json);
                Cmd.ExecuteNonQuery();
            }

            public void SetParameters(string json)
            {
                foreach (var prop in JsonDocument.Parse(json).RootElement.EnumerateObject())
                    Parameters[prop.Name].Value = prop.Value.GetString();
            }

            public void Dispose()
            {
                Cmd.Dispose();
            }
        }
    }
}
