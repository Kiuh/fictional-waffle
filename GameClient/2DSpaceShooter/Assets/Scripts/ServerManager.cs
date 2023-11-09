using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField]
    private NetworkManagerHud managerHud;

    [SerializeField]
    private HttpServer httpServer;

    [SerializeField]
    private UnityTransport transport;
    public static ushort UdpPort = 7878;
    public static ushort HttpPort = 9999;

    private void Start()
    {
#if UNITY_SERVER
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
        bool result = managerHud.StartDockerServer("127.0.0.1", UdpPort);
        Debug.Log($"Is Server: {NetworkManager.Singleton.IsServer}");
        Debug.Log(
            $"{transport.Protocol} server runed: {result} on {transport.ConnectionData.Address}:{transport.ConnectionData.Port}"
        );
        httpServer.StartHttpServer(Convert.ToString(HttpPort));
#endif
    }
}
