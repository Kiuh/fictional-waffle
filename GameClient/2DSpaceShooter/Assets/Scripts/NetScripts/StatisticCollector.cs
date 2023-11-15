using Networking;
using System;
using UnityEngine;

namespace NetScripts
{
    public class PlayerStatisticDto
    {
        public DateTime DateTime;
        public TimeSpan Duration;
        public int Kills;
        public int Deaths;
        public int Pickups;
    }

    public class StatisticCollector : MonoBehaviour
    {
        public static StatisticCollector Instance { get; private set; }

        private PlayerStatisticDto playerStatistic;
        public PlayerStatisticDto PlayerStatisticDto => playerStatistic;

        private void Awake()
        {
            if (ServerManager.IsDedicatedServer)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                playerStatistic = new PlayerStatisticDto()
                {
                    DateTime = DateTime.UtcNow,
                    Duration = TimeSpan.Zero,
                    Kills = 0,
                    Deaths = 0,
                    Pickups = 0
                };
            }
        }

        private void OnDestroy()
        {
            Instance = null;
            if (!ServerManager.IsDedicatedServer)
            {
                playerStatistic.Duration = DateTime.UtcNow - playerStatistic.DateTime;
                _ = ServerProvider.Instance.StartCoroutine(
                    ServerProvider.Instance.SendPlayerStatistic(playerStatistic)
                );
            }
        }
    }
}
