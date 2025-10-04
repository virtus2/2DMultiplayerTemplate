using Unity.Netcode;
using UnityEngine;


public enum EMainMenuState
{
    None,

    Main,
    Singleplayer,
    Multiplayer,
    MultiplayerJoinIP,

    Option,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private NetworkManager networkManager;
    private ConnectionManager connectionManager;

    [SerializeField] private NetworkObject playerCharacterPrefab;
    [SerializeField] private NetworkObject aiCharacterPrefab;

    [Header("MainMenu")]
    // MainMenu
    [SerializeField] private EMainMenuState mainMenuState;
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
        mainMenuState = EMainMenuState.Main;
    }

    private void OnDestroy()
    {
    }

    private void ChangeMainMenu(EMainMenuState state)
    {
        mainMenuState = state;
    }


    private void OnGUI()
    {
        if(connectionManager.CurrentConnectionState == EConnectionState.Offline)
        {
            switch (mainMenuState)
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
                        ChangeMainMenu(EMainMenuState.Main);
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
                        ChangeMainMenu(EMainMenuState.Multiplayer);
                    }
                    break;

                case EMainMenuState.Option:
                    if (GUILayout.Button("Back"))
                    {
                        ChangeMainMenu(EMainMenuState.Main);
                    }
                    break;
                default:

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
        CreatePlayerCharacter(clientId, Vector3.zero, Quaternion.identity);
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
