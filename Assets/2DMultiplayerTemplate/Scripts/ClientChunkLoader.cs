using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientChunkLoader : MonoBehaviour
{
    public static int ChunkSizeX = 8;
    public static int ChunkSizeY = 8;

    [SerializeField] private Grid chunkGrid;

    [Header("Sector")]
    private HashSet<Vector2Int> loadedChunk = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> chunksToLoad = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> newChunks = new HashSet<Vector2Int>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static Vector2Int GetChunkPosition(Vector2 worldPosition)
    {
        // Client chunk size is same as server
        return ServerChunkLoader.GetChunkPosition(worldPosition);
    }

    public void UpdateChunk(Vector2Int prevChunk, Vector2Int currChunk)
    {
        Debug.Log($"UpdateClientSector prevChunk:{prevChunk} currChunk:{currChunk}");
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

        // Find sectors to unload
        chunksToLoad.Clear();
        foreach (Vector2Int chunk in loadedChunk)
        {
            if (!newChunks.Contains(chunk))
            {
                chunksToLoad.Add(chunk);
            }
        }

        // Unload sectors
        foreach (Vector2Int chunk in chunksToLoad)
        {
            UnloadChunk(chunk);
            loadedChunk.Remove(chunk);
        }

        // Find sectors to load
        foreach (Vector2Int chunk in newChunks)
        {
            if (!loadedChunk.Contains(chunk))
            {
                LoadChunk(chunk);
                loadedChunk.Add(chunk);
            }
        }
    }


    private void UnloadChunk(Vector2Int chunk)
    {
    }

    private void LoadChunk(Vector2Int chunk)
    {
    }

}
