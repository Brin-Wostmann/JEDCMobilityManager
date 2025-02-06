
using System.IO.Compression;

namespace JEDCMobilityManager.Utility
{
    internal static class FileInfoExtensions
    {
        public static IEnumerable<string>EnumerateGzLines(this FileInfo file)
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
}
