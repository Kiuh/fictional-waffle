using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public enum JWTTokenMode
    {
        Enable,
        Disable
    }

    public class UnityWebRequestBuilder
    {
        private string baseURI;
        private string jwtToken;

        public UnityWebRequestBuilder(string baseURI)
        {
            this.baseURI = baseURI;
        }

        public void SetToken(string jwtToken)
        {
            this.jwtToken = jwtToken;
        }

        public UnityWebRequest CreateRequest(
            string path,
            HttpMethod method,
            object content = null,
            JWTTokenMode jwt = JWTTokenMode.Disable
        )
        {
            UnityWebRequest request =
                new(baseURI + path, method.StringValue())
                {
                    downloadHandler = new DownloadHandlerBuffer()
                };
            request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

            if (content != null)
            {
                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(JsonUtility.ToJson(content))
                );
            }

            if (jwt == JWTTokenMode.Enable)
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            return request;
        }
    }
}
