using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
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

    [Header("Test")]
    public int BuildingIndex = 0;
    public int BuildingLength = 12;

    [Header("Sector")]
    public static int SectorSizeX = 8;
    public static int SectorSizeY = 8;
    private Grid sectorGrid;
    private HashSet<Vector2Int> loadedSectors = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> sectorsToUnload = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> newSectors = new HashSet<Vector2Int>();

    private Dictionary<ulong, HashSet<Vector2Int>> loadedSectorsByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<ulong, HashSet<Vector2Int>> sectorsToUnloadByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<ulong, HashSet<Vector2Int>> newSectorsByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, List<NetworkObject>> sectorObjects = new Dictionary<Vector2Int, List<NetworkObject>>();

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

    public void HandleClientConnected(ulong clientId)
    {
        Vector3 spawnPosition = Random.insideUnitCircle * 3f;
        CreatePlayerCharacter(clientId, spawnPosition, Quaternion.identity);

        loadedSectorsByClientIds.Add(clientId, new HashSet<Vector2Int>());
        sectorsToUnloadByClientIds.Add(clientId, new HashSet<Vector2Int>());
        newSectorsByClientIds.Add(clientId, new HashSet<Vector2Int>());
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

        AddSectorObject(obj);
    }

    public Vector2Int GetSectorPosition(Vector2 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / SectorSizeX);
        int y = Mathf.FloorToInt(worldPosition.y / SectorSizeY);
        return new Vector2Int(x, y);
    }

    public void UpdateClientSector(Vector2Int prevSector, Vector2Int currSector)
    {
        Debug.Log($"UpdateClientSector prevSector:{prevSector} currSector:{currSector}");
        newSectors.Clear();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int x = currSector.x + i;
                int y = currSector.y + j;
                Vector2Int neighborSector = new Vector2Int(x, y);
                newSectors.Add(neighborSector);
            }
        }

        // Find sectors to unload
        sectorsToUnload.Clear();
        foreach (Vector2Int sector in loadedSectors)
        {
            if (!newSectors.Contains(sector))
            {
                sectorsToUnload.Add(sector);
            }
        }

        // Unload sectors
        foreach (Vector2Int sector in sectorsToUnload)
        {
            UnloadSector(sector);
            loadedSectors.Remove(sector);
        }

        // Find sectors to load
        foreach (Vector2Int sector in newSectors)
        {
            if (!loadedSectors.Contains(sector))
            {
                LoadSector(sector);
                loadedSectors.Add(sector);
            }
        }
    }


    private void UnloadSector(Vector2Int sector)
    {
    }

    private void LoadSector(Vector2Int sector)
    {
    }
    
    private void AddSectorObject(NetworkObject networkObject)
    {
        Vector2Int sectorPosition = GetSectorPosition(networkObject.transform.position);

        if (sectorObjects.TryGetValue(sectorPosition, out var objects))
        {
            objects.Add(networkObject);
        }
        else
        {
            objects = new List<NetworkObject>();
            objects.Add(networkObject);
            sectorObjects.Add(sectorPosition, objects);
        }
    }

    public void RemoveSectorObject(NetworkObject networkObject)
    {

    }

    public void UpdateServerSector(ulong clientId, Vector2Int prevSector, Vector2Int currSector)
    {
        Debug.Log($"UpdateServerSector clientId:{clientId} prevSector:{prevSector} currSector:{currSector}");
        HashSet<Vector2Int> loadedSectors = loadedSectorsByClientIds[clientId];
        HashSet<Vector2Int> sectorsToUnload = sectorsToUnloadByClientIds[clientId];
        HashSet<Vector2Int> newSectors = newSectorsByClientIds[clientId];

        newSectors.Clear();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int x = currSector.x + i;
                int y = currSector.y + j;
                Vector2Int neighborSector = new Vector2Int(x, y);
                newSectors.Add(neighborSector);
            }
        }

        // Find sectors to unload
        sectorsToUnload.Clear();
        foreach (Vector2Int sector in loadedSectors)
        {
            if (!newSectors.Contains(sector))
            {
                sectorsToUnload.Add(sector);
            }
        }

        // Unload sectors
        foreach (Vector2Int sector in sectorsToUnload)
        {
            DespawnSectorObjects(clientId, sector);
            loadedSectors.Remove(sector);
        }

        // Find sectors to load
        foreach (Vector2Int sector in newSectors)
        {
            if (!loadedSectors.Contains(sector))
            {
                SpawnSectorObjects(clientId, sector);
                loadedSectors.Add(sector);
            }
        }
    }

    private void DespawnSectorObjects(ulong clientId, Vector2Int sector)
    {
        if (sectorObjects.TryGetValue(sector, out var networkObjects))
        {
            foreach (var obj in networkObjects)
            {
                obj.NetworkHide(clientId);
            }
        }
    }

    private void SpawnSectorObjects(ulong clientId, Vector2Int sector)
    {
        if (sectorObjects.TryGetValue(sector, out var networkObjects))
        {
            foreach (var obj in networkObjects)
            {
                obj.NetworkShow(clientId);
            }
        }
    }

    public void RequestQuit()
    {
        Application.Quit();
    }
}
