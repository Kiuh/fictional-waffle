using RoomManagerApi;

namespace RoomManager
{
    internal static class NetworkClient
    {
        private static readonly HttpClient client;

        static NetworkClient()
        {
            client = new HttpClient { BaseAddress = new Uri("http://google.com") };
        }

        public static RoomPartialInfoDto? TryGetRoomPartialInfo(Uri room)
        {
            Uri uri = new(room, "Info");

            HttpResponseMessage? res;
            try
            {
                res = client.GetAsync(uri).Result;
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
