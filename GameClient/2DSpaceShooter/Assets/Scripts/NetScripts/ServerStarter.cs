using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerStarter : MonoBehaviour
{
    public bool StartDockerServer(string address, ushort port)
    {
        NetworkManager.Singleton
            .GetComponent<UnityTransport>()
            .SetConnectionData(address, port, "0.0.0.0");
        bool result = NetworkManager.Singleton.StartServer();
        return result;
    }
}
