using RoomManagerApi;

namespace RoomManager
{
	internal static class NetworkClient
	{
		private static HttpClient client;

		static NetworkClient()
		{
			client = new HttpClient();
			client.BaseAddress = new Uri("http://google.com");
		}

		public static RoomPartialInfoDto TryGetRoomPartialInfo(Uri room)
		{
			var uri = new Uri(room, "Info");

			HttpResponseMessage res = null;
			try
			{
				res = client.GetAsync(uri).Result;
            }
			catch (Exception e)
			{
				return null;
			}
			
			if (!res.IsSuccessStatusCode)
			{
				return null;
            }

			return res.Content.ReadFromJsonAsync<RoomPartialInfoDto>().Result;
        }
	}
}
