using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField]
    private NetworkManagerHud managerHud;
    public static bool isDocker = false;
    public static int httpPort = 7777;

    private void Start()
    {
#if UNITY_SERVER
        Debug.Log(
            "Command Line Args: \n"
                + Environment.GetCommandLineArgs().Aggregate("", (x, y) => x + "\n" + y)
                + "\n--------------"
        );

        List<string> arguments = Environment.GetCommandLineArgs().Skip(1).ToList();
        isDocker = true;

        managerHud.StartDockerServer(arguments[1], arguments[0]);
        Debug.Log(
            "Port and connection seated: " + arguments[1] + " " + arguments[0] + " --------------"
        );
        httpPort = int.Parse(arguments[1]);
        managerHud.StartServer();
#endif
    }
}
