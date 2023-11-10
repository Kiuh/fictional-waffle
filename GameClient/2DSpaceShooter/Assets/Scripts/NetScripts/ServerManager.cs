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

        if (arguments.Count < 2 || arguments.Count > 3)
        {
            return;
        }

        Debug.Log("UDP and HTTP ports: " + arguments[0] + " " + arguments[1] + " --------------");

        UdpPort = ushort.Parse(arguments[0]);
        HttpPort = ushort.Parse(arguments[1]);
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
        _ = NetworkManager.Singleton.SceneManager.LoadScene("network", LoadSceneMode.Single);
        return result;
    }
}
