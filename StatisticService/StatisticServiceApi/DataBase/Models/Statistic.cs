using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticServiceApi.DataBase.Models
{
    public class Statistic
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long UserId { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Pickups { get; set; }
    }
}
