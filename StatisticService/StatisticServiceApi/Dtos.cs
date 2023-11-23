namespace StatisticServiceApi
{
    public class PlayerStatisticDto
    {
        public DateTime DateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Pickups { get; set; }
    }

    public class PlayerStatisticWithIdDto
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

    public class StatisticWithIdList
    {
        public required List<PlayerStatisticWithIdDto> StatisticCells { get; set; }
    }
}
