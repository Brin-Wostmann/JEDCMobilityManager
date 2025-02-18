namespace JEDCMobilityManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //LoadData();
            LoadShapes();
        }

        public static void LoadData()
        {
            var start = DateTime.Now;
            Console.WriteLine($"[{start}] Loading Data...");
            new DataLoader(@"D:\JEDC Data\quadrant-io-mobility-data").Start();
            var end = DateTime.Now;
            Console.WriteLine($"[{end}] Done!");
            var ellapsed = end - start;
            Console.WriteLine($"{ellapsed.Hours}h {ellapsed.Minutes}m {ellapsed.Seconds}s");
        }

        public static void LoadShapes()
        {
            var start = DateTime.Now;
            Console.WriteLine($"[{start}] Loading Shapes...");
            new ShapeLoader(@"D:\JEDC Data\DistrictBoundaries_WKT_WGS84_WKID4326.csv").Start();
            var end = DateTime.Now;
            Console.WriteLine($"[{end}] Done!");
            var ellapsed = end - start;
            Console.WriteLine($"{ellapsed.Hours}h {ellapsed.Minutes}m {ellapsed.Seconds}s");
        }
    }
}
