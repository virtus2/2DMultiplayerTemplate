using Netcode.Transports.Facepunch;
using Steamworks.Data;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public enum EConnectionState
{
    Offline,
    Online,
}

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;
    public EConnectionState CurrentConnectionState;

    [SerializeField] private int maxConnectedPlayers = 4;

    private NetworkManager networkManager;
    private Lobby? currentLobby;
    private bool isInLobby;

    public bool IsSteam => connectionMethod is ConnectionMethodSteam;
    ConnectionMethod connectionMethod;

    [Header("Unity Transport")]
    [SerializeField] private UnityTransport unityTransport;
    [SerializeField] private string ipAddress;
    [SerializeField] private ushort port;
    [SerializeField] private string playerName;

    [Header("Steam Transport")]
    [SerializeField] private FacepunchTransport facepunchTransport;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR 
        unityTransport.gameObject.SetActive(true);
        unityTransport.enabled = true;
#endif

#if !UNITY_EDITOR
        facepunchTransport.gameObject.SetActive(true);
        facepunchTransport.enabled = true;
#endif
    }

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.LogLevel = LogLevel.Developer;
        CurrentConnectionState = EConnectionState.Offline;

#if UNITY_EDITOR 
        connectionMethod = new ConnectionMethodUnityTransport(this, maxConnectedPlayers, ipAddress, port);
#endif

#if !UNITY_EDITOR
        connectionMethod = new ConnectionMethodSteam(this, maxConnectedPlayers, facepunchTransport);
#endif

        Application.quitting += connectionMethod.HandleApplicationQuit;
        networkManager.OnClientConnectedCallback += HandleClientConnected;
        networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        networkManager.OnServerStopped += HandleOnServerStopped;
    }


    private void OnDisable()
    {
        CurrentConnectionState = EConnectionState.Offline;

        networkManager.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void OnApplicationQuit()
    {

    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"HandleClientConnected: clientId({clientId})");
        if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        bool hasAuthority = networkManager.IsServer || networkManager.IsHost;
        if (hasAuthority)
        {
            Debug.Log($"CreatePlayerCharacter: clientId({clientId})");
            GameManager.Instance.HandleClientConnected(clientId);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log($"clientId({clientId}) disconnected");
    }

    private void HandleOnServerStopped(bool isHost)
    {
        bool isDedicatedServer = !isHost;
    }

    public void StartServer()
    {
        connectionMethod.SetupHostConnection();
        bool success = networkManager.StartServer();
        if (success)
        {
            Debug.Log("StartServer success");
            CurrentConnectionState = EConnectionState.Online;
        }
        else
        {
            Debug.Log("StartServer failed");
        }
    }

    public void StartHost()
    {
        connectionMethod.SetupHostConnection();
        bool success = networkManager.StartHost();
        if(success)
        {
            connectionMethod.OnHostStartedSuccessfully();
            CurrentConnectionState = EConnectionState.Online;
        }
        else
        {
            connectionMethod.OnHostStartFailed();
        }
    }

    public void StartClient()
    {
        connectionMethod.SetupClientConnection();
        bool success = networkManager.StartClient();
        if (success)
        {
            Debug.Log("StartClient success");
            CurrentConnectionState = EConnectionState.Online;
        }
        else
        {
            Debug.Log("StartClient failed");
        }
    }

    public void StartClientIP(string ipAddress, ushort port)
    {
        if (connectionMethod is ConnectionMethodUnityTransport unityConnection)
        {
            unityConnection.SetIpAddress(ipAddress);
            unityConnection.SetPort(port);
            StartClient();
        }
    }

    public void ShowSteamFriendOverlay()
    {
        if (connectionMethod is ConnectionMethodSteam steamConnection)
        {
            steamConnection.ShowSteamFriendOverlay();
        }
    }

    public void OpenFriendOverlayForGameInvite()
    {
        if (connectionMethod is ConnectionMethodSteam steamConnection)
        {
            steamConnection.OpenFriendOverlayForGameInvite();
        }
    }

    public void Disconnect()
    {
        CurrentConnectionState = EConnectionState.Offline;
        connectionMethod.SetupDisconnect();
        networkManager.Shutdown();
    }
}
