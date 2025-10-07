using Netcode.Transports.Facepunch;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public enum EConnectionState
{
    Offline,
    
    StartingHost,
    Hosting,
    
    ClientConnecting,
    ClientConnected,

    StartingServer,
}

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    public ConnectionMethod ConnectionMethod { get; private set; }
    public bool IsSteam => ConnectionMethod is ConnectionMethodSteam;

    public EConnectionState CurrentConnectionState;


    [SerializeField] private int maxConnectedPlayers = 4;

    private NetworkManager networkManager;

    private ConnectionState currentState;
    private Dictionary<EConnectionState, ConnectionState> connectionStates = new Dictionary<EConnectionState, ConnectionState>();
    private OfflineState offlineState;
    private StartingHostState startingHostState;
    private HostingState hostingState;
    private ClientConnectingState clientConnectingState;
    private ClientConnectedState clientConnectedState;


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

    // TODO: Dedicated Server
#if UNITY_SERVER
#endif

#if !UNITY_EDITOR
        facepunchTransport.gameObject.SetActive(true);
        facepunchTransport.enabled = true;
#endif
    }

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.LogLevel = Unity.Netcode.LogLevel.Developer;

        offlineState = new OfflineState(this);
        startingHostState = new StartingHostState(this);
        hostingState = new HostingState(this);
        clientConnectingState = new ClientConnectingState(this);
        clientConnectedState = new ClientConnectedState(this);

        connectionStates.Add(EConnectionState.Offline, offlineState);
        connectionStates.Add(EConnectionState.StartingHost, startingHostState);
        connectionStates.Add(EConnectionState.Hosting, hostingState);
        connectionStates.Add(EConnectionState.ClientConnecting, clientConnectingState);
        connectionStates.Add(EConnectionState.ClientConnected, clientConnectedState);

        CurrentConnectionState = EConnectionState.Offline;

        currentState = offlineState;

#if UNITY_EDITOR
        ConnectionMethod = new ConnectionMethodUnityTransport(this, maxConnectedPlayers, ipAddress, port);
#endif

#if !UNITY_EDITOR
        connectionMethod = new ConnectionMethodSteam(this, maxConnectedPlayers, facepunchTransport);
#endif

        Application.quitting += ConnectionMethod.HandleApplicationQuit;
        networkManager.OnConnectionEvent += HandleConnectionEvent;
        networkManager.ConnectionApprovalCallback += HandleConnectionApproval;
        networkManager.OnServerStarted += HandleServerStarted;
        networkManager.OnServerStopped += HandleServerStopped;
        networkManager.OnTransportFailure += HandleTransportFailure;
    }

    private void OnDisable()
    {
        CurrentConnectionState = EConnectionState.Offline;

        Application.quitting -= ConnectionMethod.HandleApplicationQuit;
        networkManager.OnConnectionEvent -= HandleConnectionEvent;
        networkManager.ConnectionApprovalCallback -= HandleConnectionApproval;
        networkManager.OnServerStarted -= HandleServerStarted;
        networkManager.OnServerStopped -= HandleServerStopped;
    }

    private void OnApplicationQuit()
    {

    }

    public void ChangeState(EConnectionState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        CurrentConnectionState = newState;
        currentState = connectionStates[newState];
        currentState.Enter();
    }

    private void HandleConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        switch (connectionEventData.EventType)
        {
            case ConnectionEvent.ClientConnected:
                HandleClientConnected(connectionEventData.ClientId);
                break;
            case ConnectionEvent.ClientDisconnected:
                HandleClientDisconnected(connectionEventData.ClientId);
                break;
        }
    }

    private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        currentState.HandleConnectionApproval(request, response);
    }

    private void HandleClientConnected(ulong clientId)
    {
        currentState.HandleClientConnected(clientId);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"clientId({clientId}) disconnected");
        currentState.HandleClientDisconnected(clientId);
    }
    private void HandleServerStarted()
    {
        currentState.HandleServerStarted();
    }
    private void HandleServerStopped(bool isHost)
    {
        Debug.Log($"Server stopped: isHost({isHost})");
        currentState.HandleServerStopped(isHost);
    }

    public void StartServer()
    {
        currentState.StartServer();
    }

    public void StartHost()
    {
        currentState.StartHost();
    }

    public void StartClient()
    {
        currentState.StartClient();
    }

    public void StartClientIP(string ipAddress, ushort port)
    {
        if (ConnectionMethod is ConnectionMethodUnityTransport unityConnection)
        {
            unityConnection.SetIpAddress(ipAddress);
            unityConnection.SetPort(port);
            currentState.StartClient();
        }
    }

    public void ShowSteamFriendOverlay()
    {
        if (ConnectionMethod is ConnectionMethodSteam steamConnection)
        {
            steamConnection.ShowSteamFriendOverlay();
        }
    }

    public void OpenFriendOverlayForGameInvite()
    {
        if (ConnectionMethod is ConnectionMethodSteam steamConnection)
        {
            steamConnection.OpenFriendOverlayForGameInvite();
        }
    }

    public void Disconnect()
    {
        currentState.Disconnect();
    }

    private void HandleTransportFailure()
    {
        currentState.Disconnect();
    }
}
