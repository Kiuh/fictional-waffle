using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoomManagerApi;

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

        private static string GetServerConnectionData(string containerName)
        {
            List<DockerNetworkClient.ContainerInfo> containers =
                DockerNetworkClient.GetContainersInfo();
            foreach (DockerNetworkClient.ContainerInfo container in containers)
            {
                if (container.Name == "/" + containerName)
                {
                    RoomPartialInfoDto? additional_info = NetworkClient.TryGetRoomPartialInfo(
                        container.Uri
                    );
                    _ = additional_info.Capacity <= additional_info.ActiveUsers;

                    string[] s = container.Uri.ToString().Split(':');
                    _ = ushort.TryParse(s[2], out ushort port);
                    ServerConnectionData rett =
                        new()
                        {
                            Ipv4Address = s[0] + ":" + s[1],
                            Port = port,
                            IsFull = false
                        };
                    string retts = JsonConvert.SerializeObject(rett);
                    return retts;
                }
            }

            ServerConnectionData ret =
                new()
                {
                    Ipv4Address = "",
                    Port = 0,
                    IsFull = true
                };
            string res = JsonConvert.SerializeObject(ret);
            return res;
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
            string res = JsonConvert.SerializeObject(ret);
            return res;
        }
    }
}
