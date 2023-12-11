using StatisticService;
using StatisticServiceApi.DataBase.Models;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void StatisticsWriteValidationTest()
        {
            Statistic stats  = new Statistic() { Duration = TimeSpan.FromMinutes(5) }; 
            Assert.IsTrue(StatisticController.ValidateStatisticsWrite(stats));

            stats = new Statistic() { Duration = TimeSpan.FromDays(500) };
            Assert.IsFalse(StatisticController.ValidateStatisticsWrite(stats));
        }

        [Test]
        public void StatisticsReadValidationTest()
        {
            Statistic stats = new Statistic() { Duration = TimeSpan.FromMinutes(5) };
            Assert.IsTrue(StatisticController.ValidateStatisticsRead(stats));

            stats = new Statistic() { Duration = TimeSpan.FromDays(500) };
            Assert.IsFalse(StatisticController.ValidateStatisticsRead(stats));
        }
    }
}