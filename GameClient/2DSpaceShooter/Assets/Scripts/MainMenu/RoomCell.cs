using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    internal class RoomCell : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameLabel;

        [SerializeField]
        private TMP_Text playerCountLabel;

        [SerializeField]
        private Button joinButton;

        public void InitRoomCell(string roomName, int activeUsers, int capacity, Action onClick)
        {
            nameLabel.text = roomName;
            playerCountLabel.text = $"{activeUsers}/{capacity}";
            joinButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
