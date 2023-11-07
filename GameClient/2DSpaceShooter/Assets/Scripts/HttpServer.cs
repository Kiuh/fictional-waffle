using System;
using System.IO;
using System.Net;
using UnityEngine;

public class HttpServer : MonoBehaviour
{
    private HttpListener _httpListener;

    private void Start()
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://*:{ServerManager.httpPort}/");
        _httpListener.Start();
        _ = _httpListener.BeginGetContext(new AsyncCallback(OnGetCallback), null);
    }

    private void OnGetCallback(IAsyncResult result)
    {
        HttpListenerContext context = _httpListener.EndGetContext(result);
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        Debug.Log("\nHTTP received: " + request.Url);
        Debug.Log("HTTP UserHostAddress: " + request.UserHostAddress + "\n");

        context.Response.Headers.Clear();
        try
        {
            CreateResponse(response, new NetworkAnswer() { status = 200 });
        }
        catch (Exception e)
        {
            CreateErrorResponse(response, e.Message);
        }
        if (_httpListener.IsListening)
        {
            _ = _httpListener.BeginGetContext(new AsyncCallback(OnGetCallback), null);
        }
    }

    private async void CreateResponse(HttpListenerResponse response, NetworkAnswer data = default)
    {
        response.SendChunked = false;
        response.StatusCode = data.status;
        response.StatusDescription = data.status == 200 ? "OK" : "Internal Server Error";
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
                JsonUtility.ToJson(new NetworkAnswer() { status = 500, errorMessage = error })
            );
        }
        response.Close();
    }
}

public class NetworkAnswer
{
    public int status = 200;
    public string errorMessage = "No Error";
    public object data = "Sample data";
}
