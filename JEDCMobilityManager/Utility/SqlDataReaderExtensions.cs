
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager.Utility
{
    internal static class SqlDataReaderExtensions
    {
        public static IDictionary<string, object> ToDictionary(this SqlDataReader reader)
        {
            return reader.ToKeyValue().ToDictionary();
        }

        public static IEnumerable<KeyValuePair<string, object>> ToKeyValue(this SqlDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i++)
                yield return KeyValuePair.Create(reader.GetName(i), reader.GetValue(i));
        }
    }
}
