using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerChunkLoader : MonoBehaviour
{
    public static int ChunkSizeX = 8;
    public static int ChunkSizeY = 8;

    [SerializeField] private Grid chunkGrid;

    private Dictionary<ulong, HashSet<Vector2Int>> loadedChunksByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<ulong, HashSet<Vector2Int>> ChunksToUnloadByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<ulong, HashSet<Vector2Int>> newChunksByClientIds = new Dictionary<ulong, HashSet<Vector2Int>>();
    private Dictionary<Vector2Int, List<NetworkObject>> chunkObjects = new Dictionary<Vector2Int, List<NetworkObject>>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static Vector2Int GetChunkPosition(Vector2 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / ChunkSizeX);
        int y = Mathf.FloorToInt(worldPosition.y / ChunkSizeY);
        return new Vector2Int(x, y);
    }

    public void HandleClientConnected(ulong clientId)
    {
        loadedChunksByClientIds.Add(clientId, new HashSet<Vector2Int>());
        ChunksToUnloadByClientIds.Add(clientId, new HashSet<Vector2Int>());
        newChunksByClientIds.Add(clientId, new HashSet<Vector2Int>());
    }

    public void AddObjectToChunk(NetworkObject networkObject)
    {
        Vector2Int chunkPosition = GetChunkPosition(networkObject.transform.position);

        if (chunkObjects.TryGetValue(chunkPosition, out var objects))
        {
            objects.Add(networkObject);
        }
        else
        {
            objects = new List<NetworkObject>();
            objects.Add(networkObject);
            chunkObjects.Add(chunkPosition, objects);
        }
    }

    public void RemoveObjectFromChunk(NetworkObject networkObject)
    {
        Vector2Int chunkPosition = GetChunkPosition(networkObject.transform.position);

        if (chunkObjects.TryGetValue(chunkPosition, out var objects))
        {
            objects.Remove(networkObject);
        }
    }

    public void UpdateChunk(ulong clientId, Vector2Int prevChunk, Vector2Int currChunk)
    {
        Debug.Log($"[Server] UpdateChunk clientId:{clientId} prevChunk:{prevChunk} currChunk:{currChunk}");
        HashSet<Vector2Int> loadedChunks = loadedChunksByClientIds[clientId];
        HashSet<Vector2Int> chunksToUnload = ChunksToUnloadByClientIds[clientId];
        HashSet<Vector2Int> newChunks = newChunksByClientIds[clientId];

        newChunks.Clear();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int x = currChunk.x + i;
                int y = currChunk.y + j;
                Vector2Int neighborChunk = new Vector2Int(x, y);
                newChunks.Add(neighborChunk);
            }
        }

        // Find chunks to unload
        chunksToUnload.Clear();
        foreach (Vector2Int chunk in loadedChunks)
        {
            if (!newChunks.Contains(chunk))
            {
                chunksToUnload.Add(chunk);
            }
        }

        // Unload chunks
        foreach (Vector2Int chunk in chunksToUnload)
        {
            DespawnChunkObjects(clientId, chunk);
            loadedChunks.Remove(chunk);
        }

        // Find chunks to load
        foreach (Vector2Int chunk in newChunks)
        {
            if (!loadedChunks.Contains(chunk))
            {
                SpawnChunkObjects(clientId, chunk);
                loadedChunks.Add(chunk);
            }
        }
    }

    private void DespawnChunkObjects(ulong clientId, Vector2Int chunk)
    {
        if (chunkObjects.TryGetValue(chunk, out var networkObjects))
        {
            foreach (var obj in networkObjects)
            {
                if (!obj.SpawnWithObservers)
                    obj.NetworkHide(clientId);
            }
        }
    }

    private void SpawnChunkObjects(ulong clientId, Vector2Int chunk)
    {
        if (chunkObjects.TryGetValue(chunk, out var networkObjects))
        {
            foreach (var obj in networkObjects)
            {
                if(!obj.SpawnWithObservers)
                    obj.NetworkShow(clientId);
            }
        }
    }

    public static bool CheckObjectVisibility(ulong clientId, NetworkObject obj)
    {
        if (!NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.TryGetComponent<Player>(out var player)) return false;
        if (!player.TryGetPlayerCharacter(out var playerCharacter)) return false;

        Vector2Int characterChunkPosition = GetChunkPosition(playerCharacter.GameObject.transform.position);
        Vector2Int objectChunkPosition = GetChunkPosition(obj.transform.position);
        int distanceX = Mathf.Abs(characterChunkPosition.x - objectChunkPosition.x);
        int distanceY = Mathf.Abs(characterChunkPosition.y - objectChunkPosition.y);
        return distanceX >= 0 && distanceX < 2 && distanceY >= 0 && distanceY < 2;
    }

    public static bool IsNeighborChunk(in Vector2Int src, in Vector2Int dst)
    {
        Vector2Int diff = dst - src;
        return diff.sqrMagnitude <= 4;
    }
}
