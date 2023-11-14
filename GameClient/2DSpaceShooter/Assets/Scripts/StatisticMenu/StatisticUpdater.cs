using General;
using Networking;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace StatisticMenu
{
    internal class StatisticUpdater : MonoBehaviour
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private StatisticCell statisticCellPrefab;

        [SerializeField]
        private TMP_Text matchesPlayedLabel;

        [SerializeField]
        private TMP_Text overallKillsLabel;

        [SerializeField]
        private TMP_Text timePlayedLabel;

        [SerializeField]
        private TMP_Text overallDeathsLabel;

        [SerializeField]
        private TMP_Text overallPickupsLabel;

        [SerializeField]
        private TMP_Text error;

        private void Start()
        {
            error.text = "";
            UpdateStatistic();
        }

        private void UpdateStatistic()
        {
            while (container.childCount > 0)
            {
                Destroy(container.GetChild(0).gameObject);
            }
            _ = StartCoroutine(ServerProvider.Instance.GetAllPlayerStatistic(UpdateStatisticEnd));
            LoadingPause.Instance.ShowLoading("Refreshing...");
        }

        private void UpdateStatisticEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                StatisticList roomsList = JsonUtility.FromJson<StatisticList>(
                    webRequest.downloadHandler.text
                );
                int allKills = 0;
                int allDeaths = 0;
                int allPickups = 0;
                TimeSpan allTime = TimeSpan.Zero;
                for (int i = 0; i < roomsList.StatisticCells.Count; i++)
                {
                    StatisticCell statisticCell = Instantiate(statisticCellPrefab, container);
                    StatisticCellDto dto = roomsList.StatisticCells[i];
                    statisticCell.Initialize(
                        i + 1,
                        dto.DateTime,
                        dto.Duration,
                        dto.Kills,
                        dto.Deaths,
                        dto.Pickups
                    );
                    allKills += dto.Kills;
                    allDeaths += dto.Deaths;
                    allPickups += dto.Pickups;
                    allTime += dto.Duration;
                }
                matchesPlayedLabel.text = $"Matches played: {roomsList.StatisticCells.Count}";
                overallKillsLabel.text = $"Overall kills: {allKills}";
                timePlayedLabel.text = $"Time played: {allTime.Minutes}m {allTime.Seconds}s";
                overallDeathsLabel.text = $"Overall deaths: {allDeaths}";
                overallPickupsLabel.text = $"Overall pickups: {allPickups}";
            }
            else
            {
                error.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }

        private class StatisticList
        {
            public List<StatisticCellDto> StatisticCells;
        }

        private class StatisticCellDto
        {
            public DateTime DateTime;
            public TimeSpan Duration;
            public int Kills;
            public int Deaths;
            public int Pickups;
        }
    }
}
