using RoomManagerApi;
using System.Net.Http.Json;

namespace AdminClient
{
    internal static class RoomManagerClient
    {
        private static readonly HttpClient client;

        static RoomManagerClient()
        {
            client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5044") };
        }

        public static List<RoomInfoDto> GetRooms()
        {
            HttpResponseMessage res = client.GetAsync("/Rooms").Result;
            return res.Content.ReadFromJsonAsync<List<RoomInfoDto>>().Result;
        }

        public static async Task<bool> DeployRoom(string containerName, int capacity, string name)
        {
            JsonContent body = JsonContent.Create(
                new RoomDeployRequestDto()
                {
                    Capacity = capacity,
                    ContainerName = containerName,
                    Name = name,
                }
            );
            HttpResponseMessage res = client.PutAsync("/Rooms", body).GetAwaiter().GetResult();
            return await res.Content.ReadAsStringAsync() == "true";
        }

        public static async Task<bool> DropRoom(string containerName)
        {
            HttpResponseMessage res = client
                .DeleteAsync($"/Room/{containerName}")
                .GetAwaiter()
                .GetResult();
            return await res.Content.ReadAsStringAsync() == "true";
        }
    }
}
