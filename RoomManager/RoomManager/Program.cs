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

		private static List<RoomInfoDto> GetRooms()
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

			return rooms;
		}
    }
}