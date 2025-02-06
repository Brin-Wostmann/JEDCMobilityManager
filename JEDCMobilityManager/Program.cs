namespace JEDCMobilityManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var start = DateTime.Now;
            Console.WriteLine($"[{start}] Running...");
            new DataLoader(@"D:\JEDC Data\quadrant-io-mobility-data").Start();
            var end = DateTime.Now;
            Console.WriteLine($"[{end}] Done!");
            var ellapsed = end - start;
            Console.WriteLine($"{ellapsed.Hours}h {ellapsed.Minutes}m {ellapsed.Seconds}s");
        }
    }
}
