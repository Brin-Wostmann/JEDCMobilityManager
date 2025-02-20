using System.Data;
using System.Globalization;
using CsvHelper;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class ShapeLoader : SqlScript
    {
        private string ShapeFile { get; set; }

        public ShapeLoader(string shapeFile)
        {
            ShapeFile = shapeFile;
        }

        public override void Start(string connection)
        {
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();
                using (var transation = sql.BeginTransaction())
                using (var cmd = new SqlCommand("INSERT INTO [dbo].[Area] ([Name], [Shape]) VALUES (@name, geography::STGeomFromText(@shape, 4326))", sql, transation))
                using (var reader = new StreamReader(ShapeFile))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var nameParam = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    var shapeParam = cmd.Parameters.Add("@shape", SqlDbType.NVarChar);

                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        nameParam.Value = csv.GetField("districtname");
                        shapeParam.Value = csv.GetField("SHAPE@WKT");
                        cmd.ExecuteNonQuery();
                    }

                    transation.Commit();
                }
                sql.Close();
            }
        }
    }
}
