
namespace JEDCMobilityManager.Utility
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<string> ReadLines(this IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            foreach (var line in file.EnumerateGzLines())
                yield return line;
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            var more = true;
            using (var enumerator = source.GetEnumerator())
                do yield return Inner(enumerator);
                while (more);

            IEnumerable<T> Inner(IEnumerator<T> enumerator)
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
