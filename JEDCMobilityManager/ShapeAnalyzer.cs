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
        private ConcurrentQueue<Tuple<int, int>> ResultQueue { get; set; } = null!;
        private SemaphoreSlim ResultQueueSemaphore { get; set; } = null!;
        private bool FinishedPoints { get; set; }

        public override void Start(string connection)
        {
            FinishedPoints = false;
            ResultQueue = new ConcurrentQueue<Tuple<int, int>>();
            ResultQueueSemaphore = new SemaphoreSlim(0);
            Areas = Area.GetAll(connection);
            Envelope = Areas.FindIntersections();
            var resultsTask = Task.Run(() => ProcessResults(connection));
            RunPoints(EnumeratePoints(connection));
            FinishedPoints = true;
            ResultQueueSemaphore.Release();
            resultsTask.Wait();
        }

        private void RunPoints(IEnumerable<Tuple<int, Point>> points)
        {
            var count = 0;
            foreach (var point in points)
            {
                if (++count % 100000 == 0)
                    Console.WriteLine(count);

                if (!Envelope.Contains(point.Item2.Coordinate)) continue;
                foreach (var area in Areas)
                {
                    if (!TestContains(area)) continue;
                    foreach (var areaIntersect in area.Intersects)
                        TestContains(areaIntersect);
                    break;
                }

                bool TestContains(Area area)
                {
                    if (!area.Envelope.Contains(point.Item2.Coordinate)) return false;
                    if (!area.Geometry.Contains(point.Item2)) return false;
                    EnqueueResult(point.Item1, area.Id);
                    return true;
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

        public void EnqueueResult(int personId, int areaId)
        {
            ResultQueue.Enqueue(Tuple.Create(personId, areaId));
            ResultQueueSemaphore.Release();
        }

        public async Task ProcessResults(string connection)
        {
            const string command =
                @"IF NOT EXISTS (
                    SELECT 1 
                    FROM [dbo].[PersonArea] 
                    WHERE [PersonId] = @personId AND [AreaId] = @areaId
                )
                BEGIN
                    INSERT INTO [dbo].[PersonArea] ([PersonId], [AreaId]) 
                    VALUES (@personId, @areaId)
                END";

            using (var sql = new SqlConnection(connection))
            using (var cmd = new SqlCommand(command, sql))
            {
                var personId = cmd.Parameters.Add("@personId", SqlDbType.Int);
                var areaId = cmd.Parameters.Add("@areaId", SqlDbType.Int);
                sql.Open();
                while (!FinishedPoints || !ResultQueue.IsEmpty)
                {
                    await ResultQueueSemaphore.WaitAsync();
                    if (ResultQueue.TryDequeue(out var item))
                    {
                        personId.Value = item.Item1;
                        areaId.Value = item.Item2;
                        cmd.ExecuteNonQuery();
                    }
                }
                sql.Close();
            }
        }
    }
}
