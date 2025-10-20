using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public enum EMainMenuState
{
    None,

    Main,
    Singleplayer,
    Multiplayer,
    MultiplayerJoinIP,

    SelectSaveFile,
    NewSave,

    Option,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public ClientProjectile ClientProjectilePrefab;
    public NetworkObject ServerProjectilePrefab;
    [SerializeField] private NetworkObject playerCharacterPrefab;
    [SerializeField] private NetworkObject aiCharacterPrefab;
    [SerializeField] private NetworkObject projectilePrefab;

    private NetworkManager networkManager;
    private ConnectionManager connectionManager;

    [Header("MainMenu")]
    // MainMenu
    [SerializeField] private EMainMenuState prevMainMenuState;
    [SerializeField] private EMainMenuState currMainMenuState;
    private string ipAddress = "127.0.0.1";
    private string port = "7777";
    
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
                        connectionManager.ShowSteamFriendOverlay();
                    }
                    if (GUILayout.Button("Host"))
                    {
                        connectionManager.StartHost();
                        ChangeMainMenu(EMainMenuState.None);
                    }
                    if (GUILayout.Button("Back"))
                    {
                        BackToPreviousMenu();
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
            if (GUILayout.Button("Disconnect"))
            {
                ChangeMainMenu(EMainMenuState.Main);
                connectionManager.Disconnect();
            }
        }
    }

    public void HandleClientConnected(ulong clientId)
    {
        Vector3 spawnPosition = Random.insideUnitCircle * 3f;
        CreatePlayerCharacter(clientId, spawnPosition, Quaternion.identity);
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

    public void RequestQuit()
    {
        Application.Quit();
    }
}
