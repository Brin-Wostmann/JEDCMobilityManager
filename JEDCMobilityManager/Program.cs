namespace JEDCMobilityManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var start = DateTime.Now;
            Console.WriteLine($"[{start}] Running...");
            new SqliteDataLoader().Start();
            var end = DateTime.Now;
            Console.WriteLine($"[{end}] Done!");
            var ellapsed = end - start;
            Console.WriteLine($"{ellapsed.Hours}h {ellapsed.Minutes}m {ellapsed.Seconds}s");
        }
    }
}
