using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(NetworkManager))]
[DisallowMultipleComponent]
public class NetworkManagerHud : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

    [SerializeField]
    private UnityTransport m_Transport;

    // This is needed to make the port field more convenient. GUILayout.TextField is very limited and we want to be able to clear the field entirely so we can't cache this as ushort.
    private string m_PortString = "10000";
    private string m_ConnectAddress = "127.0.0.1";

    public void StartDockerServer(string udpPort, string httpPort) { }

    [SerializeField]
    private UIDocument m_MainMenuUIDocument;

    [SerializeField]
    private UIDocument m_InGameUIDocument;
    private VisualElement m_MainMenuRootVisualElement;
    private VisualElement m_InGameRootVisualElement;
    private Button m_HostButton;
    private Button m_ServerButton;
    private Button m_ClientButton;
    private Button m_ShutdownButton;
    private TextField m_IPAddressField;
    private TextField m_PortField;
    private TextElement m_MenuStatusText;
    private TextElement m_InGameStatusText;

    private void Awake()
    {
        // Only cache networking manager but not transport here because transport could change anytime.
        m_NetworkManager = GetComponent<NetworkManager>();

        m_MainMenuRootVisualElement = m_MainMenuUIDocument.rootVisualElement;

        m_IPAddressField = m_MainMenuRootVisualElement.Q<TextField>("IPAddressField");
        m_PortField = m_MainMenuRootVisualElement.Q<TextField>("PortField");
        m_HostButton = m_MainMenuRootVisualElement.Q<Button>("HostButton");
        m_ClientButton = m_MainMenuRootVisualElement.Q<Button>("ClientButton");
        m_ServerButton = m_MainMenuRootVisualElement.Q<Button>("ServerButton");
        m_MenuStatusText = m_MainMenuRootVisualElement.Q<TextElement>("ConnectionStatusText");

        m_InGameRootVisualElement = m_InGameUIDocument.rootVisualElement;
        m_ShutdownButton = m_InGameRootVisualElement.Q<Button>("ShutdownButton");
        m_InGameStatusText = m_InGameRootVisualElement.Q<TextElement>("InGameStatusText");

        m_IPAddressField.value = m_ConnectAddress;
        m_PortField.value = m_PortString;

        m_HostButton.clickable.clickedWithEventInfo += HostButtonClicked;
        m_ServerButton.clickable.clickedWithEventInfo += ServerButtonClicked;
        m_ClientButton.clickable.clickedWithEventInfo += ClientButtonClicked;
        m_ShutdownButton.clickable.clickedWithEventInfo += ShutdownButtonClicked;

        ShowMainMenuUI(true);
        ShowInGameUI(false);
        ShowStatusText(false);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnOnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
    }

    private void OnOnClientConnectedCallback(ulong obj)
    {
        ShowMainMenuUI(false);
        ShowInGameUI(true);
    }

    private void OnOnClientDisconnectCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.ServerClientId)
        {
            return;
        }
        ShowMainMenuUI(true);
        ShowInGameUI(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsRunning(NetworkManager networkManager)
    {
        return networkManager.IsServer || networkManager.IsClient;
    }

    public bool SetConnectionData()
    {
        if (!ServerManager.isDocker)
        {
            m_ConnectAddress = SanitizeInput(m_IPAddressField.value);
            m_PortString = SanitizeInput(m_PortField.value);
        }
        else
        {
            m_ConnectAddress = "0.0.0.0";
        }

        if (m_ConnectAddress == "")
        {
            m_MenuStatusText.text = "IP Address Invalid";
            StopAllCoroutines();
            _ = StartCoroutine(ShowInvalidInputStatus());
            return false;
        }

        if (m_PortString == "")
        {
            m_MenuStatusText.text = "Port Invalid";
            StopAllCoroutines();
            _ = StartCoroutine(ShowInvalidInputStatus());
            return false;
        }

        Debug.Log($"m_PortString: {m_PortString}");
        if (ushort.TryParse(m_PortString, out ushort port))
        {
            Debug.Log($"Try set address: {m_ConnectAddress} and port: {port}");
            m_Transport.SetConnectionData(m_ConnectAddress, port);
        }
        else
        {
            Debug.Log($"Try set address: {m_ConnectAddress} and port: {7777}");
            m_Transport.SetConnectionData(m_ConnectAddress, 7777);
        }
        return true;
    }

    private static string SanitizeInput(string dirtyString)
    {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^0-9.]", "");
    }

    private void HostButtonClicked(EventBase obj)
    {
        if (SetConnectionData())
        {
            _ = NetworkManager.Singleton.StartHost();
        }
    }

    private void ClientButtonClicked(EventBase obj)
    {
        if (SetConnectionData())
        {
            _ = NetworkManager.Singleton.StartClient();
            StopAllCoroutines();
            _ = StartCoroutine(ShowConnectingStatus());
        }
    }

    public void ServerButtonClicked(EventBase obj)
    {
        StartServer();
    }

    public void StartServer()
    {
        if (SetConnectionData())
        {
            bool success = m_NetworkManager.StartServer();
            Debug.Log($"Server started: {success}");
            Debug.Log($"ACTIVE ON PORT: {m_Transport.ConnectionData.Port}");
            Debug.Log($"ACTIVE ON Address: {m_Transport.ConnectionData.Address}");
            Debug.Log(
                $"ACTIVE ON ServerListenAddress: {m_Transport.ConnectionData.ServerListenAddress}"
            );
            ShowMainMenuUI(false);
            ShowInGameUI(true);
        }
    }

    private void ShutdownButtonClicked(EventBase obj)
    {
        m_NetworkManager.Shutdown();
        ShowMainMenuUI(true);
        ShowInGameUI(false);
    }

    private void ShowStatusText(bool visible)
    {
        m_MenuStatusText.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private IEnumerator ShowInvalidInputStatus()
    {
        ShowStatusText(true);

        yield return new WaitForSeconds(3f);

        ShowStatusText(false);
    }

    private IEnumerator ShowConnectingStatus()
    {
        m_MenuStatusText.text = "Attempting to Connect...";
        ShowStatusText(true);

        m_HostButton.SetEnabled(false);
        m_ServerButton.SetEnabled(false);

        UnityTransport unityTransport = m_NetworkManager.GetComponent<UnityTransport>();
        int connectTimeoutMs = unityTransport.ConnectTimeoutMS;
        int maxConnectAttempts = unityTransport.MaxConnectAttempts;

        yield return new WaitForSeconds(connectTimeoutMs * maxConnectAttempts / 1000f);

        // wait to verify connect status
        yield return new WaitForSeconds(1f);

        m_MenuStatusText.text = "Connection Attempt Failed";
        m_HostButton.SetEnabled(true);
        m_ServerButton.SetEnabled(true);

        yield return new WaitForSeconds(3f);

        ShowStatusText(false);
    }

    private void ShowMainMenuUI(bool visible)
    {
        m_MainMenuRootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ShowInGameUI(bool visible)
    {
        m_InGameRootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (m_NetworkManager.IsServer)
        {
            string mode = m_NetworkManager.IsHost ? "Host" : "Server";
            m_InGameStatusText.text = $"ACTIVE ON PORT: {m_Transport.ConnectionData.Port}";
            m_ShutdownButton.text = $"Shutdown {mode}";
        }
        else
        {
            if (m_NetworkManager.IsConnectedClient)
            {
                m_InGameStatusText.text =
                    $"CONNECTED {m_Transport.ConnectionData.Address} : {m_Transport.ConnectionData.Port}";
                m_ShutdownButton.text = "Shutdown Client";
            }
        }
    }

    private void OnDestroy()
    {
        if (m_HostButton != null)
        {
            m_HostButton.clickable.clickedWithEventInfo -= HostButtonClicked;
        }

        if (m_ServerButton != null)
        {
            m_ServerButton.clickable.clickedWithEventInfo -= ServerButtonClicked;
        }

        if (m_ClientButton != null)
        {
            m_ClientButton.clickable.clickedWithEventInfo -= ClientButtonClicked;
        }

        if (m_ShutdownButton != null)
        {
            m_ShutdownButton.clickable.clickedWithEventInfo -= ShutdownButtonClicked;
        }
        m_NetworkManager.OnClientConnectedCallback -= OnOnClientConnectedCallback;
        m_NetworkManager.OnClientDisconnectCallback -= OnOnClientDisconnectCallback;
    }
}
