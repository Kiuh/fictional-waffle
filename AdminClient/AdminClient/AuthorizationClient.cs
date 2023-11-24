using AuthorizationApi;
using System.Net;
using System.Net.Http.Json;

namespace AdminClient
{
    internal static class AuthorizationClient
    {
        private static readonly HttpClient client;

        static AuthorizationClient()
        {
            client = new HttpClient { BaseAddress = new Uri("https://127.0.0.1:5000") };
        }

        public static async Task<string> GetPubkey()
        {
            HttpResponseMessage res = await client.GetAsync("/PublicKey");
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<HttpStatusCode> Login(LoginDto data)
        {
            HttpResponseMessage res = await client.PostAsJsonAsync("/Login", data);
            return res.StatusCode;
        }

        public static async Task<HttpStatusCode> Register(RegistrationDto data)
        {
            HttpResponseMessage res = await client.PutAsJsonAsync("/Registration", data);
            return res.StatusCode;
        }
    }
}
