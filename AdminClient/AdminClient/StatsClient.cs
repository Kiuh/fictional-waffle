using System.Net.Http.Json;

namespace AdminClient
{
    public class PlayerStatisticDto
    {
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Pickups { get; set; }
    }

    public class StatisticList
    {
        public required List<PlayerStatisticDto> StatisticCells { get; set; }
    }

    internal static class StatsClient
    {
        private static readonly HttpClient client;

        static StatsClient()
        {
            client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5005") };
        }

        public static List<PlayerStatisticDto> GetPlayerStats()
        {
            HttpResponseMessage res = client.GetAsync("/AllStatistic").Result;
            return res.Content.ReadFromJsonAsync<StatisticList>().Result.StatisticCells;
        }
    }
}
