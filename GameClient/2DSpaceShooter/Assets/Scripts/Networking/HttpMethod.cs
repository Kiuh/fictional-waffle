using System;
using UnityEngine.Networking;

namespace Networking
{
    public enum HttpMethod
    {
        Get,
        Delete,
        Post,
        Put,
        Create,
        Patch
    }

    public static class HttpMethodTools
    {
        public static string StringValue(this HttpMethod method)
        {
            return method switch
            {
                HttpMethod.Get => UnityWebRequest.kHttpVerbGET,
                HttpMethod.Delete => UnityWebRequest.kHttpVerbDELETE,
                HttpMethod.Post => UnityWebRequest.kHttpVerbPOST,
                HttpMethod.Put => UnityWebRequest.kHttpVerbPUT,
                HttpMethod.Create => UnityWebRequest.kHttpVerbCREATE,
                HttpMethod.Patch => "PATCH",
                _ => throw new ArgumentException()
            };
        }
    }
}
