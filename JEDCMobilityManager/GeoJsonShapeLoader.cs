using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace JEDCMobilityManager
{
    internal class GeoJsonShapeLoader : SqlScript
    {
        private string ShapeFile { get; }
        private string Group { get; }

        public GeoJsonShapeLoader(string shapeFile, string group)
        {
            ShapeFile = shapeFile;
            Group = group;
        }

        public override void Start(string connection)
        {
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();
                using (var transation = sql.BeginTransaction())
                using (var cmd = new SqlCommand("INSERT INTO [dbo].[Area] ([Name], [Shape], [Group]) VALUES (@name, geography::STGeomFromText(@shape, 4326), @group)", sql, transation))
                using (var jdoc = JsonDocument.Parse(File.ReadAllText(ShapeFile)))
                {
                    var geoReader = new GeoJsonReader();
                    var nameParam = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    var shapeParam = cmd.Parameters.Add("@shape", SqlDbType.NVarChar);
                    var groupParam = cmd.Parameters.Add("@group", SqlDbType.NVarChar);

                    foreach (var feature in jdoc.RootElement.GetProperty("features").EnumerateArray())
                    {
                        nameParam.Value = feature.GetProperty("properties").GetProperty("CommunityName").GetString();
                        shapeParam.Value = geoReader.Read<Geometry>(feature.GetProperty("geometry").ToString()).ToText();
                        groupParam.Value = Group;
                        cmd.ExecuteNonQuery();
                    }

                    transation.Commit();
                }
                sql.Close();
            }
        }
    }
}