using Microsoft.EntityFrameworkCore;
using StatisticServiceApi.DataBase.Models;

namespace StatisticServiceApi.DataBase
{
    public class StatisticDbContext : DbContext
    {
        public DbSet<Statistic> Statistics { get; set; } = null!;

        public StatisticDbContext(DbContextOptions<StatisticDbContext> options)
            : base(options)
        {
            _ = Database.EnsureCreated();
            Console.WriteLine("Database EnsureCreated");
        }
    }
}
