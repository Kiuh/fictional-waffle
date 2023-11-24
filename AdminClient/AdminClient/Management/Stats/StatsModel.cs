using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AdminClient.Management.Stats
{
    public partial class UserStats: ObservableObject
    {
        [ObservableProperty]
        private int id = 0;

        [ObservableProperty]
        private int kills = 0;

        [ObservableProperty]
        private int deaths = 0;

        [ObservableProperty]
        private int pickups = 0;
    }

    public partial class StatsModel: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<UserStats> users = new ObservableCollection<UserStats>();

        public void Init()
        {
            var stats = StatsClient.GetPlayerStats();
            foreach (var stat in stats)
            {
                bool user_exists = false;

                foreach (var user in Users)
                {
                    if (user.Id == stat.Id)
                    {
                        user.Pickups += stat.Pickups;
                        user.Kills += stat.Kills;
                        user.Deaths += stat.Deaths;

                        user_exists = true;

                        break;
                    }
                }

                if (!user_exists)
                {
                    Users.Add(new UserStats() { Id = (int)stat.Id, Kills = stat.Kills, Deaths = stat.Deaths, Pickups = stat.Pickups });
                }
            }
        }
    }
}
