using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private NetworkObject playerCharacterPrefab;
    [SerializeField] private NetworkObject aiCharacterPrefab;

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
            else
            {
                if (GUILayout.Button("Create AI Character"))
                {
                    Vector2 randomPosition = Random.insideUnitCircle * 5f;
                    CreateAICharacter(randomPosition, Quaternion.identity);
                }
                if (GUILayout.Button("Create AI Character x100"))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 randomPosition = Random.insideUnitCircle * 5f;
                        CreateAICharacter(randomPosition, Quaternion.identity);
                    }
                }
                if (GUILayout.Button("Create AI Character x1000"))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        Vector2 randomPosition = Random.insideUnitCircle * 5f;
                        CreateAICharacter(randomPosition, Quaternion.identity);
                    }
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

    private void CreateAICharacter(in Vector3 position, in Quaternion rotation)
    {
        aiCharacterPrefab.InstantiateAndSpawn(networkManager,
            ownerClientId: 0,
            position: position,
            rotation: rotation
        );
    }
}
