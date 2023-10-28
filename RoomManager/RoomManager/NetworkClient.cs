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

		public static RoomPartialInfoDto GetRoomPartialInfo(Uri room)
		{
			var uri = new Uri(room, "Info");
			var res = client.GetAsync(uri).Result;
		}
	}
}
