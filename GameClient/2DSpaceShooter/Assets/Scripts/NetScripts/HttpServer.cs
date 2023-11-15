using System;
using System.IO;
using System.Net;
using Unity.Netcode;
using UnityEngine;

public class HttpServer : MonoBehaviour
{
    [SerializeField]
    private int maxConnections;
    private HttpListener httpListener;

    public void StartHttpServer(string httpPort)
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://*:{httpPort}/Info/");
        httpListener.Start();
        _ = httpListener.BeginGetContext(new AsyncCallback(OnGetCallback), null);
    }

    private void OnGetCallback(IAsyncResult result)
    {
        HttpListenerContext context = httpListener.EndGetContext(result);
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        Debug.Log("\nHTTP received: " + request.Url);
        Debug.Log("HTTP UserHostAddress: " + request.UserHostAddress + "\n");

        context.Response.Headers.Clear();
        try
        {
            CreateResponse(
                response,
                new RoomPartialInfoDto()
                {
                    Name = ServerManager.ServerName,
                    ActiveUsers = NetworkManager.Singleton.PendingClients.Count,
                    Capacity = ServerManager.ServerCapacity
                }
            );
        }
        catch (Exception e)
        {
            CreateErrorResponse(response, e.Message);
        }
        if (httpListener.IsListening)
        {
            _ = httpListener.BeginGetContext(new AsyncCallback(OnGetCallback), null);
        }
    }

    private async void CreateResponse(HttpListenerResponse response, RoomPartialInfoDto data)
    {
        response.SendChunked = false;
        response.StatusCode = 200;
        response.StatusDescription = "OK";
        using (StreamWriter writer = new(response.OutputStream, response.ContentEncoding))
        {
            await writer.WriteAsync(JsonUtility.ToJson(data));
        }
        response.Close();
    }

    private async void CreateErrorResponse(HttpListenerResponse response, string error)
    {
        response.SendChunked = false;
        response.StatusCode = 500;
        response.StatusDescription = "Internal Server Error";
        using (StreamWriter writer = new(response.OutputStream, response.ContentEncoding))
        {
            await writer.WriteAsync(
                JsonUtility.ToJson(new NetworkAnswer() { Status = 500, ErrorMessage = error })
            );
        }
        response.Close();
    }
}

public class NetworkAnswer
{
    public int Status = 200;
    public string ErrorMessage = "No Error";
    public object Data = "Sample data";
}

public class RoomPartialInfoDto
{
    public string Name;
    public int ActiveUsers;
    public int Capacity;
}
