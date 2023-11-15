using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }

    [SerializeField]
    private HttpServer httpServer;

    [SerializeField]
    private UnityTransport transport;
    public static ushort UdpPort = 7878;
    public static ushort HttpPort = 9999;
    public static int ServerCapacity = -1;
    public static string ServerName = string.Empty;
    public static bool IsDedicatedServer = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log(
            "Command Line Args: "
                + Environment.GetCommandLineArgs().Aggregate("", (x, y) => x + " " + y)
                + " --------------"
        );

        List<string> arguments = Environment
            .GetCommandLineArgs()
            .Skip(1)
            .Select(x => x.Trim(','))
            .ToList();
        const int args_count = 4;
        if (arguments.Count < args_count || arguments.Count > args_count)
        {
            return;
        }

        Debug.Log("UDP and HTTP ports: " + arguments[0] + " " + arguments[1] + " --------------");
        IsDedicatedServer = true;
        UdpPort = ushort.Parse(arguments[0]);
        HttpPort = ushort.Parse(arguments[1]);
        ServerCapacity = int.Parse(arguments[2]);
        ServerName = arguments[3];

        bool result = StartDockerServer("127.0.0.1", UdpPort);
        Debug.Log($"Is Server: {NetworkManager.Singleton.IsServer}");
        Debug.Log(
            $"{transport.Protocol} server runed: {result} on {transport.ConnectionData.Address}:{transport.ConnectionData.Port}"
        );
        httpServer.StartHttpServer(Convert.ToString(HttpPort));
    }

    public bool StartDockerServer(string address, ushort port)
    {
        NetworkManager.Singleton
            .GetComponent<UnityTransport>()
            .SetConnectionData(address, port, "0.0.0.0");
        bool result = NetworkManager.Singleton.StartServer();
        _ = NetworkManager.Singleton.SceneManager.LoadScene("GamePlay", LoadSceneMode.Single);
        return result;
    }
}
