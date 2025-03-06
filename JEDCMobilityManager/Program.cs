using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class Program
    {
        private static SqlConnectionStringBuilder Connection { get; set; } = new SqlConnectionStringBuilder {
            DataSource = "localhost",
            InitialCatalog = "JEDCMobility",
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 0,
            CommandTimeout = 0
        };

        public static void Main(string[] args)
        {
            //RunScript(new DataLoader(@"D:\JEDC Data\quadrant-io-mobility-data"));
            //RunScript(new ShapeLoader(@"D:\JEDC Data\DistrictBoundaries_WKT_WGS84_WKID4326.csv"));
            //RunScript(new ShapeAnalyzer());
        }

        public static void RunScript(SqlScript script)
        {
            var name = script.GetType().Name;
            var start = DateTime.Now;
            Console.WriteLine($"[{start}] Starting {name}...");
            script.Start(Connection.ToString());
            var end = DateTime.Now;
            Console.WriteLine($"[{end}] Finished {name}.");
            var ellapsed = end - start;
            Console.WriteLine($"{name} in: {ellapsed.Hours}h {ellapsed.Minutes}m {ellapsed.Seconds}s");
        }
    }
}
