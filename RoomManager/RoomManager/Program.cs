using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using RoomManagerApi;

namespace RoomManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

			app.MapGet("Rooms", GetRooms);
			app.MapPut("Rooms", DeployRoom);
			app.MapDelete("Room/{containerName}", UndeployRoom);
			app.MapGet("Room/{containerName}", GetServerConnectionData);

            app.Run("http://0.0.0.0:5044");
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
            var containers = DockerNetworkClient.GetContainersInfo();
			foreach(var container in containers)
			{
				if (container.Name == containerName)
				{
                    var additional_info = NetworkClient.TryGetRoomPartialInfo(container.Uri);
					var full = additional_info.Capacity <= additional_info.ActiveUsers;

                    var s = container.Uri.ToString().Split(':');
                    ushort.TryParse(s[1], out ushort port);
                    return new ServerConnectionData() { Ipv4Address = s[0], Port = port, IsFull = full };
                }
            }

            return new ServerConnectionData() { Ipv4Address = "", Port = 0, IsFull = true };
        }

		private static RoomInfosDto GetRooms()
		{
			var containers = DockerNetworkClient.GetContainersInfo();
			var rooms = new List<RoomInfoDto>();
			foreach (var room in containers)
			{
				var additional_info = NetworkClient.TryGetRoomPartialInfo(room.Uri);
				if (additional_info == null)
				{
					continue;
				}

                rooms.Add(new RoomInfoDto()
				{
					ActiveUsers = additional_info.ActiveUsers, 
					Capacity = additional_info.Capacity, 
					ContainerName = room.Name, 
					DeployedAt = room.DeployedAt, 
					Name = additional_info.Name,
				});
			}

			var ret = new RoomInfosDto();
			ret.RoomsDtoList = rooms;
			return ret;
		}
    }
}