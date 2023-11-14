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
                "/Rooms",
                HttpMethod.Get,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }

        public IEnumerator GetServerConnectionData(
            Action<UnityWebRequest> action,
            string serverName
        )
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                $"/Rooms/{serverName}",
                HttpMethod.Get,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }
    }
}
