using General;
using Networking;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MainMenu
{
    internal class RoomListUpdater : MonoBehaviour
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private Button refreshButton;

        [SerializeField]
        private RoomCell roomCellPrefab;

        [SerializeField]
        private TMP_Text errorLabel;

        private void Awake()
        {
            errorLabel.text = "";
            refreshButton.onClick.AddListener(Refresh);
        }

        private void Refresh()
        {
            while (container.childCount > 0)
            {
                Destroy(container.GetChild(0).gameObject);
            }
            _ = StartCoroutine(ServerProvider.Instance.GetAllAvailableRooms(RefreshEnd));
            LoadingPause.Instance.ShowLoading("Refreshing...");
        }

        private void RefreshEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                RoomsList roomsList = JsonUtility.FromJson<RoomsList>(
                    webRequest.downloadHandler.text
                );
                foreach (RoomInfoDto dto in roomsList.RoomsDtoList)
                {
                    RoomCell roomCell = Instantiate(roomCellPrefab, container);
                    roomCell.InitRoomCell(
                        dto.Name,
                        dto.ActiveUsers,
                        dto.Capacity,
                        () => ConnectToServer(dto.ContainerName)
                    );
                }
            }
            else
            {
                errorLabel.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }

        public class RoomsList
        {
            public List<RoomInfoDto> RoomsDtoList;
        }

        public class RoomInfoDto
        {
            public string Name;
            public string ContainerName;
            public DateTime DeployedAt;
            public int ActiveUsers;
            public int Capacity;
        }

        private void ConnectToServer(string serverName)
        {
            _ = StartCoroutine(
                ServerProvider.Instance.GetServerConnectionData(ConnectToServerEnd, serverName)
            );
            LoadingPause.Instance.ShowLoading("Connecting...");
        }

        private void ConnectToServerEnd(UnityWebRequest webRequest)
        {
            LoadingPause.Instance.HideLoading();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                ServerConnectionData connectionData = JsonUtility.FromJson<ServerConnectionData>(
                    webRequest.downloadHandler.text
                );
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData(connectionData.Ipv4Address, connectionData.Port);
                _ = NetworkManager.Singleton.StartClient();
            }
            else
            {
                errorLabel.text = webRequest.downloadHandler.text.FromErrorBody();
            }
        }

        private class ServerConnectionData
        {
            public string Ipv4Address;
            public ushort Port;
        }
    }
}
