using System.Collections.Concurrent;
using System.Data;
using JEDCMobilityManager.Utility;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;

namespace JEDCMobilityManager
{
    internal class ShapeAnalyzer : SqlScript
    {
        private IList<Area> Areas { get; set; } = null!;
        private Envelope Envelope { get; set; } = null!;
        private ResultManager Results { get; set; } = null!;

        public override void Start(string connection)
        {
            Results = new ResultManager(connection);
            // ReSharper disable once AccessToDisposedClosure
            var rTask = Task.Run(() => Results.LoadExisting());

            Areas = Area.GetAll(connection);
            Envelope = Areas.FindIntersections();

            rTask.Wait();
            // ReSharper disable once AccessToDisposedClosure
            rTask = Task.Run(() => Results.StartProcessing());

            RunPoints(EnumeratePoints(connection));
            Results.StopProcessing();
            rTask.Wait();
            Results.Dispose();
        }

        private void RunPoints(IEnumerable<Tuple<int, Point>> points)
        {
            var maxThreads = Environment.ProcessorCount;
            var chunkQueue = new ConcurrentQueue<IEnumerable<Tuple<int, Point>>>();
            var chunkSignal = new SemaphoreSlim(0);
            var capacitySignal = new SemaphoreSlim(maxThreads * 3, maxThreads * 3);
            var done = false;
            var pointTasks = new Task[maxThreads];
            for (var i = 0; i < maxThreads; i++)
                pointTasks[i] = Task.Run(() => PointTask());

            //var count = 0;
            foreach (var chunk in points.Partition(100000))
            {
                capacitySignal.Wait();
                chunkQueue.Enqueue(chunk.ToList());
                chunkSignal.Release();
                //Console.WriteLine(++count * 100000);
            }

            done = true;
            chunkSignal.Release(maxThreads);
            Task.WaitAll(pointTasks);
            //Console.WriteLine("All Points Processed");

            void PointTask()
            {
                // ReSharper disable once AccessToModifiedClosure
                while (!done || !chunkQueue.IsEmpty)
                {
                    chunkSignal.Wait();
                    try { capacitySignal.Release(); }catch { /**/ }
                    if (!chunkQueue.TryDequeue(out var chunk))
                        continue;

                    foreach (var point in chunk)
                    {
                        if (!Envelope.Contains(point.Item2.Coordinate)) continue;
                        var known = Results.GetKnown(point.Item1);
                        foreach (var area in Areas)
                        {
                            if (!TestContains(area)) continue;
                            foreach (var areaIntersect in area.Intersects)
                                TestContains(areaIntersect);
                            break;
                        }

                        bool TestContains(Area area)
                        {
                            if (known.Contains(area.Id)) return false;
                            if (!area.Envelope.Contains(point.Item2.Coordinate)) return false;
                            if (!area.Geometry.Contains(point.Item2)) return false;
                            Results.Add(point.Item1, area.Id);
                            return true;
                        }
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
                        yield return Tuple.Create(reader.GetInt32(0), new Point((double) reader.GetDecimal(2), (double) reader.GetDecimal(1)));

                sql.Close();
            }
        }

        private class ResultManager : IDisposable
        {
            private SqlConnection Sql { get; }
            private IDictionary<int, IList<int>> Known { get; } = new Dictionary<int, IList<int>>();
            private ConcurrentQueue<Tuple<int, int>> Imports { get; } = new ConcurrentQueue<Tuple<int, int>>();
            private SemaphoreSlim ImportSignal { get; set; } = new SemaphoreSlim(0);
            private bool FinishProcessing { get; set; }

            public ResultManager(string connection)
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
                Imports.Clear();
                ImportSignal = new SemaphoreSlim(0);
            }

            public IList<int> GetKnown(int personId)
            {
                return Known.TryGetValue(personId, out var areas) ? areas : new List<int>();
            }

            public void Add(int personId, int areaId)
            {
                lock (Known)
                {
                    if (!Known.TryGetValue(personId, out var areas))
                    {
                        areas = new List<int>();
                        Known.Add(personId, areas);
                    }
                    if (areas.Contains(areaId)) return;
                    areas.Add(areaId);
                    Imports.Enqueue(Tuple.Create(personId, areaId));
                    ImportSignal.Release();
                }
            }

            public void StopProcessing()
            {
                FinishProcessing = true;
                ImportSignal.Release();
            }

            public async Task StartProcessing()
            {
                FinishProcessing = false;
                Sql.Open();
                using (var transation = Sql.BeginTransaction())
                using (var cmd = new SqlCommand("INSERT INTO [dbo].[PersonArea] ([PersonId], [AreaId]) VALUES (@personId, @areaId)", Sql, transation))
                {
                    var personId = cmd.Parameters.Add("@personId", SqlDbType.Int);
                    var areaId = cmd.Parameters.Add("@areaId", SqlDbType.Int);
                    while (!FinishProcessing || !Imports.IsEmpty)
                    {
                        await ImportSignal.WaitAsync();
                        if (Imports.TryDequeue(out var item))
                        {
                            personId.Value = item.Item1;
                            areaId.Value = item.Item2;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transation.Commit();
                }
                Sql.Close();
            }

            public void Dispose()
            {
                Sql.Dispose();
            }
        }
    }
}
