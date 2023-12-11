using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoomManagerApi;
using System.Text.Json;

namespace RoomManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            WebApplication app = builder.Build();

            _ = app.MapGet("Rooms", GetRooms);
            _ = app.MapPut("Rooms", DeployRoom);
            _ = app.MapDelete("Room/{containerName}", UndeployRoom);
            _ = app.MapGet("Room/{containerName}", GetServerConnectionData);

            app.Run("http://0.0.0.0:5015");
        }

        private static bool DeployRoom([FromBody] RoomDeployRequestDto request)
        {
            return DockerNetworkClient.TryDeployContainer(request.ContainerName);
        }

        private static bool UndeployRoom(string containerName)
        {
            return DockerNetworkClient.TryUndeployContainer(containerName);
        }

        private static ServerConnectionData GetServerConnectionData(string containerName)
        {
            List<DockerNetworkClient.ContainerInfo> containers =
                DockerNetworkClient.GetContainersInfo();
            foreach (DockerNetworkClient.ContainerInfo container in containers)
            {
                if (container.Name == containerName)
                {
                    RoomPartialInfoDto? additional_info = NetworkClient.TryGetRoomPartialInfo(
                        container.Uri
                    );
                    bool full = additional_info.Capacity <= additional_info.ActiveUsers;

                    string[] s = container.Uri.ToString().Split(':');
                    _ = ushort.TryParse(s[1], out ushort port);
                    return new ServerConnectionData()
                    {
                        Ipv4Address = s[0],
                        Port = port,
                        IsFull = full
                    };
                }
            }

            return new ServerConnectionData()
            {
                Ipv4Address = "",
                Port = 0,
                IsFull = true
            };
        }

        private static string GetRooms()
        {
            List<DockerNetworkClient.ContainerInfo> containers =
                DockerNetworkClient.GetContainersInfo();
            List<RoomInfoDto> rooms = new();
            foreach (DockerNetworkClient.ContainerInfo room in containers)
            {
                RoomPartialInfoDto? additional_info = NetworkClient.TryGetRoomPartialInfo(room.Uri);
                if (additional_info == null)
                {
                    continue;
                }

                rooms.Add(
                    new RoomInfoDto()
                    {
                        ActiveUsers = additional_info.ActiveUsers,
                        Capacity = additional_info.Capacity,
                        ContainerName = room.Name,
                        DeployedAt = room.DeployedAt,
                        Name = additional_info.Name,
                    }
                );
            }

            RoomInfosDto ret = new() { RoomsDtoList = rooms };
            var res = JsonConvert.SerializeObject(ret);
            return res;
        }
    }
}
