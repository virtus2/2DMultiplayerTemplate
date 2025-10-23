using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


public enum EMainMenuState
{
    None,

    Main,
    Singleplayer,
    Multiplayer,
    MultiplayerJoinIP,
    MultiplayerJoinSteam,

    SelectSaveFile,
    NewSave,

    Option,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private ServerChunkLoader serverChunkLoader;
    [SerializeField] private ClientChunkLoader clientChunkLoader;

    [Header("Test")]
    public int BuildingIndex = 0;
    public int BuildingLength = 12;


    [Header("Projectiles")]
    public ClientProjectile ClientProjectilePrefab;
    public NetworkObject ServerProjectilePrefab;

    [Header("Prefabs")]
    [SerializeField] private NetworkObject playerCharacterPrefab;
    [SerializeField] private NetworkObject aiCharacterPrefab;
    [SerializeField] private NetworkObject buildingPrefab;

    private NetworkManager networkManager;
    private ConnectionManager connectionManager;

    [Header("MainMenu")]
    // MainMenu
    [SerializeField] private EMainMenuState prevMainMenuState;
    [SerializeField] private EMainMenuState currMainMenuState;
    private string ipAddress = "127.0.0.1";
    private string port = "7777";
    private string lobbyNumber = "";

    public ServerChunkLoader ServerChunkLoader
    {
        get
        {
            Debug.Assert(networkManager.IsServer);
            return serverChunkLoader;
        }
    }
    public ClientChunkLoader ClientChunkLoader
    {
        get
        {
            Debug.Assert(networkManager.IsClient);
            return clientChunkLoader;
        }
    }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        connectionManager = ConnectionManager.Instance;
        networkManager = NetworkManager.Singleton;
        prevMainMenuState = EMainMenuState.None;
        currMainMenuState = EMainMenuState.Main;
    }

    private void OnDestroy()
    {
    }

    private void ChangeMainMenu(EMainMenuState state)
    {
        prevMainMenuState = currMainMenuState;
        currMainMenuState = state;
    }
    private void BackToPreviousMenu()
    {
        ChangeMainMenu(prevMainMenuState);
    }

    private void OnGUI()
    {
        if(connectionManager.CurrentConnectionState == EConnectionState.Offline)
        {
            switch (currMainMenuState)
            {
                case EMainMenuState.Main:
                    if (GUILayout.Button("Singleplayer"))
                    {
                        ChangeMainMenu(EMainMenuState.Singleplayer);
                    }
                    if (GUILayout.Button("Multiplayer"))
                    {
                        ChangeMainMenu(EMainMenuState.Multiplayer);
                    }
                    if (GUILayout.Button("Option"))
                    {
                        ChangeMainMenu(EMainMenuState.Option);
                    }
                    if (GUILayout.Button("Quit"))
                    {
                        RequestQuit();
                    }

                    break;
                case EMainMenuState.Singleplayer:
                    if (GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
                    }
                    break;
                case EMainMenuState.Multiplayer:
                    if (GUILayout.Button("Join via IP"))
                    {
                        ChangeMainMenu(EMainMenuState.MultiplayerJoinIP);
                    }
                    if (GUILayout.Button("Join via Steam"))
                    {
                        ChangeMainMenu(EMainMenuState.MultiplayerJoinSteam);
                    }
                    if (GUILayout.Button("Host"))
                    {
                        connectionManager.StartHost();
                        ChangeMainMenu(EMainMenuState.None);
                    }
                    if (GUILayout.Button("Back"))
                    {
                        ChangeMainMenu(EMainMenuState.Main);
                    }
                    break;

                case EMainMenuState.MultiplayerJoinIP:
                    GUILayout.Label("Enter IP:");
                    ipAddress = GUILayout.TextField(ipAddress);

                    GUILayout.Label("Enter port:");
                    port = GUILayout.TextField(port);

                    if (GUILayout.Button("Join"))
                    {
                        if (ushort.TryParse(port, out ushort result))
                        {
                            connectionManager.StartClientIP(ipAddress, result);
                            ChangeMainMenu(EMainMenuState.None);
                        }
                    }
                    if (GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
                    }
                    break;

                case EMainMenuState.MultiplayerJoinSteam:
                    GUILayout.Label("Enter lobby:");
                    lobbyNumber = GUILayout.TextField(lobbyNumber);
                    if (GUILayout.Button("Enter"))
                    {
                        connectionManager.StartClientSteamLobby(lobbyNumber);
                        ChangeMainMenu(EMainMenuState.None);
                    }

                    if (GUILayout.Button("Invite friend"))
                    {
                        connectionManager.ShowSteamFriendOverlay();
                    }
                    if (GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
                    }
                    break;

                case EMainMenuState.SelectSaveFile:
                    if(GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
                    }
                    break;

                case EMainMenuState.Option:
                    if (GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
                    }
                    break;
                default:
                    ChangeMainMenu(EMainMenuState.Main);
                    break;
            }
        }
        else
        {
            GUILayout.Label($"isServer: {networkManager.IsServer}");
            GUILayout.Label($"isHost: {networkManager.IsHost}");
            GUILayout.Label($"isClient: {networkManager.IsClient}");
            if (GUILayout.Button("Invite"))
            {
                connectionManager.OpenFriendOverlayForGameInvite();
            }
            if (networkManager.IsServer)
            {
                if (GUILayout.Button("Create Building x 1000"))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        CreateBuilding(Vector3.zero, Quaternion.identity);
                    }
                }
            }
            if (GUILayout.Button("Disconnect"))
            {
                ChangeMainMenu(EMainMenuState.Main);
                connectionManager.Disconnect();
            }
        }
    }

    public void CreatePlayerCharacterOnClientConnected(ulong clientId)
    {
        Vector3 spawnPosition = Random.insideUnitCircle * 3f;
        CreatePlayerCharacter(clientId, spawnPosition, Quaternion.identity);
    }

    public void InitializeServerChunkOnClientConnected(ulong clientId)
    {
        ServerChunkLoader.HandleClientConnected(clientId);
    }

    private void CreatePlayerCharacter(ulong clientId, in Vector3 position, in Quaternion rotation)
    {
        playerCharacterPrefab.InstantiateAndSpawn(networkManager,
            ownerClientId: clientId,
            position: position,
            rotation: rotation
        );
    }

    private void CreateAICharacter(in Vector3 position, in Quaternion rotation)
    {
        aiCharacterPrefab.InstantiateAndSpawn(networkManager,
            ownerClientId: 0,
            position: position,
            rotation: rotation
        );
    }

    private void CreateBuilding(in Vector3 position, in Quaternion rotation)
    {
        int posX = BuildingIndex % BuildingLength;
        int posY = BuildingIndex / BuildingLength;
        Vector3 newPos = new Vector3(posX, posY, 0);

        // NetworkObject obj = Instantiate(buildingPrefab, newPos, Quaternion.identity);
        
        NetworkObject obj = buildingPrefab.InstantiateAndSpawn(networkManager,
            ownerClientId: 0,
            position: newPos,
            rotation: rotation
        );
        

        Building building = obj.GetComponent<Building>();
        building.StartCrafting();
        
        
        BuildingIndex++;
    }

    public void RemoveSectorObject(NetworkObject networkObject)
    {

    }

    public void RequestQuit()
    {
        Application.Quit();
    }
}
