using System;
using System.Collections;
using UnityEngine.Networking;

namespace Networking
{
    public partial class ServerProvider
    {
        public IEnumerator SayEnteringRoom(Action<UnityWebRequest> action)
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Statistic",
                HttpMethod.Put,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }
    }
}
