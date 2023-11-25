using NetScripts;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace Networking
{
    public partial class ServerProvider
    {
        public IEnumerator GetAllPlayerStatistic(Action<UnityWebRequest> action)
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Redirect/Statistic",
                HttpMethod.Get,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            action(webRequest);
            webRequest.Dispose();
        }

        public IEnumerator SendPlayerStatistic(PlayerStatisticDto playerStatistic)
        {
            UnityWebRequest webRequest = requestBuilder.CreateRequest(
                "/Redirect/Statistic",
                HttpMethod.Put,
                content: playerStatistic,
                jwt: JWTTokenMode.Enable
            );

            yield return webRequest.SendWebRequest();

            UnityEngine.Debug.Log(webRequest.result);
            webRequest.Dispose();
        }
    }
}
