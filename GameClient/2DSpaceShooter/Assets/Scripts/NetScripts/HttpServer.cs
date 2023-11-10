using System;
using System.IO;
using System.Net;
using UnityEngine;

public class HttpServer : MonoBehaviour
{
    private HttpListener httpListener;

    public void StartHttpServer(string httpPort)
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://*:{httpPort}/");
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
            CreateResponse(response, new NetworkAnswer() { Status = 200 });
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

    private async void CreateResponse(HttpListenerResponse response, NetworkAnswer data = default)
    {
        response.SendChunked = false;
        response.StatusCode = data.Status;
        response.StatusDescription = data.Status == 200 ? "OK" : "Internal Server Error";
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
