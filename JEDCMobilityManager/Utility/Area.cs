﻿using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager.Utility
{
    internal class Area
    {
        private static WKTReader Reader { get; } = new WKTReader();

        public int Id { get; }
        public Geometry Geometry { get; }
        public Envelope Envelope { get; }
        public IList<Area> Intersects { get; } = new List<Area>();

        public Area(int id, string shape)
        {
            Id = id;
            Geometry = Reader.Read(shape);
            Envelope = Geometry.EnvelopeInternal;
        }

        public static IList<Area> GetAll(string connection)
        {
            var areas = new List<Area>();
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();

                using (var cmd = new SqlCommand("SELECT [Id], [Shape].STAsText() FROM [dbo].[Area]", sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        areas.Add(new Area(reader.GetInt32(0), reader.GetString(1)));

                sql.Close();
            }

            return areas;
        }
    }

    internal static class AreaExtensions
    {
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
