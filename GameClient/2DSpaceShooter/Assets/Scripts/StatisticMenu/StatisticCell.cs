using System;
using TMPro;
using UnityEngine;

namespace StatisticMenu
{
    internal class StatisticCell : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text numberLabel;

        [SerializeField]
        private TMP_Text startTimeLabel;

        [SerializeField]
        private TMP_Text durationLabel;

        [SerializeField]
        private TMP_Text killsLabel;

        [SerializeField]
        private TMP_Text deathsLabel;

        [SerializeField]
        private TMP_Text pickupsLabel;

        public void Initialize(
            int number,
            DateTime dateTime,
            TimeSpan duration,
            int kills,
            int deaths,
            int pickups
        )
        {
            numberLabel.text = $"{number})";
            startTimeLabel.text = $"Start time: {dateTime.Day}:{dateTime.Month}:{dateTime.Year}";
            durationLabel.text = $"Duration: {duration.Minutes}m {duration.Seconds}s";
            killsLabel.text = $"Kills: {kills}";
            deathsLabel.text = $"Deaths: {deaths}";
            pickupsLabel.text = $"Pickups: {pickups}";
        }
    }
}
