using System;
using System.Collections;
using UnityEngine.Networking;

namespace Networking
{
    public partial class ServerProvider
    {
        public IEnumerator GetAllAvailableRooms(Action<UnityWebRequest> action)
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Redirect/Rooms",
                HttpMethod.Get,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }

        public IEnumerator GetServerConnectionData(
            Action<UnityWebRequest> action,
            string containerName
        )
        {
            containerName = containerName.Replace("/", "");
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                $"/Redirect/Rooms/{containerName}",
                HttpMethod.Get,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }
    }
}
