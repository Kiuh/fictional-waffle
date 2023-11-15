using Newtonsoft.Json;
using System;

namespace Networking
{
    [Serializable]
    public class ErrorBody
    {
        public string Error { get; set; }
    }

    public static class ErrorBodyTools
    {
        public static string FromErrorBody(this string body)
        {
            ErrorBody errorBody = JsonConvert.DeserializeObject<ErrorBody>(body);
            return errorBody == null ? "No response." : errorBody.Error ?? "Unknown error.";
        }
    }
}
