﻿using System.Data;
using System.Globalization;
using CsvHelper;
using Microsoft.Data.SqlClient;

namespace JEDCMobilityManager
{
    internal class ShapeLoader : SqlScript
    {
        private string ShapeFile { get; }
        private string Group { get; }

        public ShapeLoader(string shapeFile, string group)
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
                using (var reader = new StreamReader(ShapeFile))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var nameParam = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    var shapeParam = cmd.Parameters.Add("@shape", SqlDbType.NVarChar);
                    var groupParam = cmd.Parameters.Add("@group", SqlDbType.NVarChar);

                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        nameParam.Value = csv.GetField("districtname");
                        shapeParam.Value = csv.GetField("SHAPE@WKT");
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
