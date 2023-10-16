using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AuthorizationApi;

namespace AdminClient
{
    internal static class NetworkClient
	{
		private static HttpClient client;

		static NetworkClient()
		{
			client = new HttpClient();
			client.BaseAddress = new Uri("localhost:8080");
        }

		public static async Task<string> GetPubkey()
		{
			var res = await client.GetAsync("/PublicKey");
			return await res.Content.ReadAsStringAsync();
		}

		public static async Task<HttpStatusCode> Login(LoginDto data)
		{
			var res = await client.PostAsJsonAsync("/Login", data);
			return res.StatusCode;
		}

		public static async Task<HttpStatusCode> Register(RegistrationDto data)
		{
			var res = await client.PutAsJsonAsync("/Registration", data);
			return res.StatusCode;
		}
    }
}
