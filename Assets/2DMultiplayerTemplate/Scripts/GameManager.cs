using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NetworkObject playerCharacterPrefab;

    private NetworkManager networkManager;

    private void Awake()
    {

    }

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnGUI()
    {
        if(networkManager)
        {
            if(!networkManager.IsServer && !networkManager.IsClient)
            {
                if (GUILayout.Button("Start Server"))
                {
                    StartServer();
                }
                if (GUILayout.Button("Start Host"))
                {
                    StartHost();
                }
                if (GUILayout.Button("Start Client"))
                {
                    networkManager.OnClientConnectedCallback -= OnClientConnected;
                    StartClient();
                }
            }
        }
    }

    private void StartServer()
    {
        networkManager.StartServer();
    }

    private void StartHost()
    {
        networkManager.StartHost();
    }

    private void StartClient()
    {
        networkManager.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

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
}
