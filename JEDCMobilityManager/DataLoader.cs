using System.Collections.Generic;
using System.IO.Compression;

namespace JEDCMobilityManager
{
    internal class DataLoader
    {
        private static string RootPath = @"D:\JEDC Data\quadrant-io-mobility-data";

        public void Start()
        {


            foreach (var file in GetFiles(RootPath))
            {
                Console.WriteLine("[");
                foreach (var line in ReadLines(file.Item2).Take(2))
                {
                    Console.Write(line);
                    Console.WriteLine(",");
                }

                Console.WriteLine("]");
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
