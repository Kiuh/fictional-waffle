using RoomManagerApi;

namespace RoomManager
{
    internal static class NetworkClient
    {
        //private static readonly HttpClient client;

        static NetworkClient()
        {
           
        }

        public static RoomPartialInfoDto? TryGetRoomPartialInfo(Uri room)
        {
            return new RoomPartialInfoDto() { ActiveUsers = 10, Capacity = 10, Name = "Name" };

            Uri uri = new(room, "Info");
            var client = new HttpClient { BaseAddress = uri };

            HttpResponseMessage? res;
            try
            {
                res = client.GetAsync("").Result;
            }
            catch (Exception)
            {
                return null;
            }

            return !res.IsSuccessStatusCode
                ? null
                : res.Content.ReadFromJsonAsync<RoomPartialInfoDto>().Result;
        }
    }
}
