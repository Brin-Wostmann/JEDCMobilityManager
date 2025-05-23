using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Simplify;

namespace JEDCMobilityManager.Utility
{
    public class Area
    {
        private static WKTReader Reader { get; } = new WKTReader();
        private static GeoJsonWriter Writer { get; } = new GeoJsonWriter();

        public int Id { get; }
        public string Name { get; }
        public Geometry Geometry { get; private set; }
        public Envelope Envelope { get; }
        public IList<Area> Intersects { get; } = new List<Area>();
        public string GeoJson { get; }
        public IDictionary<string, object> OtherValues { get; }

        public Area(int id, string name, string shape, IDictionary<string, object> otherValues)
        {
            Id = id;
            Name = name;
            Geometry = Reader.Read(shape);
            Envelope = Geometry.EnvelopeInternal;
            GeoJson = Writer.Write(Geometry);
            OtherValues = otherValues;
        }

        public void Simplify(double tolderance = 0.01)
        {
            Geometry = TopologyPreservingSimplifier.Simplify(Geometry, tolderance);
        }

        public static IList<Area> GetAll(string connection, string? command = null)
        {
            var areas = new List<Area>();
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();

                using (var cmd = new SqlCommand(command ?? "SELECT [Id], [Name], [Shape].STAsText() FROM [dbo].[Area]", sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        areas.Add(new Area(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.ToDictionary()));

                sql.Close();
            }

            return areas;
        }
    }

    internal static class AreaExtensions
    {
        public static void Simplify(this IList<Area> areas, double tolderance = 0.01)
        {
            foreach (var area in areas)
                area.Simplify(tolderance);
        }

        public static Envelope FindIntersections(this IList<Area> areas)
        {
            var envelope = new Envelope();
            for (var i = 0; i < areas.Count; i++)
            {
                var areaA = areas[i];
                envelope.ExpandToInclude(areaA.Envelope);
                for (var j = i + 1; j < areas.Count; j++)
                {
                    var areaB = areas[j];
                    if (areaA.Geometry.Intersects(areaB.Geometry))
                    {
                        areaA.Intersects.Add(areaB);
                        areaB.Intersects.Add(areaA);
                    }
                }
            }

            return envelope;
        }
    }
}
