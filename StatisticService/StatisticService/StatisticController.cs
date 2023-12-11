using Microsoft.AspNetCore.Mvc;
using StatisticServiceApi;
using StatisticServiceApi.DataBase;
using StatisticServiceApi.DataBase.Models;

namespace StatisticService
{
    [ApiController]
    public class StatisticController : Controller
    {
        private readonly StatisticDbContext statisticDbContext;

        public StatisticController(StatisticDbContext statisticDbContext)
        {
            this.statisticDbContext = statisticDbContext;
        }

        public static bool ValidateStatisticsWrite(Statistic stats)
        {
            if(stats.Duration > TimeSpan.FromDays(1))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateStatisticsRead(Statistic stats)
        {
            if (stats.Duration > TimeSpan.FromDays(1))
            {
                return false;
            }

            return true;
        }

        [HttpPut("/Statistic")]
        public IActionResult PutStatistic(
            [FromBody] PlayerStatisticDto registrationDto,
            [FromQuery] int UserId
        )
        {
            Statistic statistic =
                new()
                {
                    UserId = UserId,
                    DateTime = registrationDto.DateTime,
                    Duration = registrationDto.Duration, // "00:00:01"
                    Kills = registrationDto.Kills,
                    Deaths = registrationDto.Deaths,
                    Pickups = registrationDto.Pickups,
                };

            if(ValidateStatisticsWrite(statistic))
            {
                _ = statisticDbContext.Statistics.Add(statistic);
            }
            
            int count = statisticDbContext.SaveChanges();
            return Ok($"Saved entities: {count}");
        }

        [HttpGet("/Statistic")]
        public IActionResult GetStatistic([FromQuery] int UserId)
        {
            List<PlayerStatisticDto> playerStatisticDtos = [];
            foreach (Statistic statistic in statisticDbContext.Statistics)
            {
                if (statistic.Id == UserId && ValidateStatisticsRead(statistic))
                {
                    playerStatisticDtos.Add(
                        new PlayerStatisticDto()
                        {
                            DateTime = statistic.DateTime,
                            Deaths = statistic.Deaths,
                            Duration = statistic.Duration,
                            Kills = statistic.Kills,
                            Pickups = statistic.Pickups
                        });
                }
            }
            StatisticList statisticList = new() { StatisticCells = playerStatisticDtos };
            return Ok(statisticList);
        }

        [HttpGet("/AllStatistic")]
        public IActionResult GetAllStatistic()
        {
            List<PlayerStatisticWithIdDto> playerStatisticDtos = statisticDbContext.Statistics
                .ToList()
                .Select(statistic => new PlayerStatisticWithIdDto()
                {
                    DateTime = statistic.DateTime,
                    Deaths = statistic.Deaths,
                    Duration = statistic.Duration,
                    Kills = statistic.Kills,
                    Pickups = statistic.Pickups,
                    Id = statistic.Id
                }).ToList();
            StatisticWithIdList statisticList = new() { StatisticCells = playerStatisticDtos };
            return Ok(statisticList);
        }
    }
}
