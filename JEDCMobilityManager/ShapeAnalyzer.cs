using System.Data;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace JEDCMobilityManager
{
    internal class ShapeAnalyzer : SqlScript
    {
        private IList<Area> Areas { get; set; } = new List<Area>();
        private Envelope Envelope { get; } = new Envelope();
        private PersonAreaManager PaManager { get; set; } = null!;

        public override void Start(string connection)
        {
            var loadAreaTask = Task.Run(() => LoadAreas(connection));
            loadAreaTask.Start();

            PaManager = new PersonAreaManager(connection);
            PaManager.LoadExisting();

            loadAreaTask.Wait();

            RunPoints(EnumeratePoints(connection));

            PaManager.Dispose();
        }

        private void RunPoints(IEnumerable<Tuple<int, Point>> data)
        {
            Parallel.ForEach(Batch(data, 10000), ProcesBatch);

            IEnumerable<IList<T>> Batch<T>(IEnumerable<T> items, int size)
            {
                using var enumerator = items.GetEnumerator();
                while (true)
                {
                    var batch = new List<T>(size);
                    lock (enumerator)
                        for (int i = 0; i < size && enumerator.MoveNext(); i++)
                            batch.Add(enumerator.Current);
                    if (batch.Count == 0)
                    {
                        PaManager.Flush();
                        yield break;
                    }
                    Task.Run(PaManager.Flush);
                    yield return batch;
                }
            }

            void ProcesBatch(IEnumerable<Tuple<int, Point>> points)
            {
                foreach (var point in points)
                {
                    if (!Envelope.Contains(point.Item2.Coordinate)) continue;
                    var knownAreas = PaManager.GetPerson(point.Item1);
                    foreach (var area in Areas)
                    {
                        if (!TestContains(area)) continue;
                        foreach (var areaIntersect in area.Intersects)
                            TestContains(areaIntersect);
                        break;
                    }

                    bool TestContains(Area area)
                    {
                        if (knownAreas.Contains(area.Id)) return false;
                        if (!area.Envelope.Contains(point.Item2.Coordinate)) return false;
                        if (!area.Geometry.Contains(point.Item2)) return false;
                        PaManager.Add(point.Item1, area.Id);
                        return true;
                    }
                }
            }
        }

        private IEnumerable<Tuple<int, Point>> EnumeratePoints(string connection)
        {
            using (var sql = new SqlConnection(connection))
            using (var cmd = new SqlCommand("SELECT [PersonId], [Latitude], [Longitude] FROM [dbo].[Point]", sql))
            {
                sql.Open();

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        yield return Tuple.Create(reader.GetInt32(0), new Point(reader.GetDouble(2), reader.GetDouble(1)));

                sql.Close();
            }
        }

        private void LoadAreas(string connection)
        {
            using (var sql = new SqlConnection(connection))
            {
                sql.Open();

                using (var cmd = new SqlCommand("SELECT [Id], [Shape].STAsText() FROM [dbo].[Area]", sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        Areas.Add(new Area(reader.GetInt32(0), reader.GetString(1)));

                sql.Close();
            }

            for (var i = 0; i < Areas.Count; i++)
            {
                var areaA = Areas[i];
                Envelope.ExpandToInclude(areaA.Envelope);
                for (var j = i + 1; j < Areas.Count; j++)
                {
                    var areaB = Areas[j];
                    if (areaA.Geometry.Intersects(areaB.Geometry))
                    {
                        areaA.Intersects.Add(areaB);
                        areaB.Intersects.Add(areaA);
                    }
                }
            }
        }

        private class Area
        {
            private static WKTReader Reader { get; } = new WKTReader();

            public int Id { get; }
            //public string WktShape { get; }
            public Geometry Geometry { get; }
            public Envelope Envelope { get; }
            public IList<Area> Intersects { get; } = new List<Area>();

            public Area(int id, string shape)
            {
                Id = id;
                //WktShape = shape;
                Geometry = Reader.Read(shape);
                Envelope = Geometry.EnvelopeInternal;
            }
        }

        private class PersonAreaManager : IDisposable
        {
            private SqlConnection Sql { get; }
            private IDictionary<int, IList<int>> PersonAreas { get; } = new Dictionary<int, IList<int>>();
            private IList<Tuple<int, int>> Inserts { get; set; } = new List<Tuple<int, int>>();

            public PersonAreaManager(string connection)
            {
                Sql = new SqlConnection(connection);
            }

            public void LoadExisting()
            {
                Sql.Open();

                using (var cmd = new SqlCommand("SELECT [PersonId], [AreaId] FROM [dbo].[PersonArea]", Sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        Add(reader.GetInt32(0), reader.GetInt32(1));

                Sql.Close();
                Inserts = new List<Tuple<int, int>>();
            }

            public IList<int> GetPerson(int personId)
            {
                return PersonAreas.TryGetValue(personId, out var areas) ? areas : new List<int>();
            }

            public void Add(int personId, int areaId)
            {
                if (!PersonAreas.TryGetValue(personId, out var areas))
                {
                    areas = new List<int>();
                    PersonAreas[personId] = areas;
                }

                if (!areas.Contains(areaId))
                {
                    areas.Add(areaId);
                    Inserts.Add(Tuple.Create(personId, areaId));
                }
            }

            public void Flush()
            {
                if (!Inserts.Any()) return;

                lock (Sql)
                {
                    var inserts = Inserts;
                    Inserts = new List<Tuple<int, int>>();

                    Sql.Open();

                    using (var transaction = Sql.BeginTransaction())
                    using (var cmd = new SqlCommand("INSERT INTO [dbo].[PersonArea] ([PersonId], [AreaId]) VALUES @personId, @areaId", Sql, transaction))
                    {
                        var personId = cmd.Parameters.Add("@personId", SqlDbType.Int);
                        var areaId = cmd.Parameters.Add("@areaId", SqlDbType.Int);

                        foreach (var insert in inserts)
                        {
                            personId.Value = insert.Item1;
                            areaId.Value = insert.Item2;
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }

                    Sql.Close();
                }
            }

            public void Dispose()
            {
                Sql.Dispose();
            }
        }
    }
}
