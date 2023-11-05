using System.Net.Http.Json;
using RoomManagerApi;

namespace AdminClient
{
	internal static class RoomManagerClient
	{
		private static HttpClient client;

		static RoomManagerClient()
		{
			client = new HttpClient();
			client.BaseAddress = new Uri("http://127.0.0.1:5044");
		}

		public static async Task<List<RoomInfoDto>> GetRooms()
		{
			var res = client.GetAsync("/Rooms").Result;
			return res.Content.ReadFromJsonAsync<List<RoomInfoDto>>().Result;
		}

		public static async Task<bool> DeployRoom(string containerName, int capacity, string name)
		{
			var body = JsonContent.Create(new RoomDeployRequestDto()
			{
				Capacity = capacity, 
				ContainerName = containerName, 
				Name = name,
			});
			var res = client.PutAsync("/Rooms", body).GetAwaiter().GetResult();
			return await res.Content.ReadAsStringAsync() == "true";
		}

		public static async Task<bool> DropRoom(string containerName)
		{
			var res = client.DeleteAsync($"/Room/{containerName}").GetAwaiter().GetResult();
			return await res.Content.ReadAsStringAsync() == "true";
		}
    }
}

