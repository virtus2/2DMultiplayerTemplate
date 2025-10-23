using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public struct CraftInfo : INetworkSerializable
{
    public float StartTime;
    public float CraftTime;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref StartTime);
        serializer.SerializeValue(ref CraftTime);
    }
}

public class Building : NetworkBehaviour, IInteractable, IDamageable
{
    [Header("Gameplay events")]
    public UnityEvent OnTakeDamage;

    private NetworkVariable<CraftInfo> craftInfo = new NetworkVariable<CraftInfo>();
    private float elapsedTime;
    private float progress;
    private float endTime;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        if (HasAuthority)
        {
            NetworkObject.CheckObjectVisibility += CheckObjectVisibility;
            GameManager.Instance.ServerChunkLoader.AddObjectToChunk(NetworkObject);
        }
        else
        {
            craftInfo.OnValueChanged += OnCraftInfoChanged;

            float spawnTime = NetworkManager.LocalTime.TimeAsFloat;
            elapsedTime = spawnTime - craftInfo.Value.StartTime;
            endTime = craftInfo.Value.StartTime + craftInfo.Value.CraftTime;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (HasAuthority)
        {
            NetworkObject.CheckObjectVisibility -= CheckObjectVisibility;
            GameManager.Instance.ServerChunkLoader.RemoveObjectFromChunk(NetworkObject);
        }
        else
        {
            craftInfo.OnValueChanged -= OnCraftInfoChanged;
        }
    }

    private bool CheckObjectVisibility(ulong clientId)
    {
        return ServerChunkLoader.CheckObjectVisibility(clientId, NetworkObject);
    }

    private void OnCraftInfoChanged(CraftInfo prev, CraftInfo curr)
    {
        float spawnTime = NetworkManager.LocalTime.TimeAsFloat;
        elapsedTime = spawnTime - craftInfo.Value.StartTime;
        endTime = craftInfo.Value.StartTime + craftInfo.Value.CraftTime;
    }

    private void Update()
    {
        if (elapsedTime + craftInfo.Value.StartTime >= endTime)
        {
            elapsedTime = 0f;

            if (HasAuthority)
            {
                StartCrafting();
            }
        }
        progress = elapsedTime / (endTime - craftInfo.Value.StartTime);
        spriteRenderer.color = Color.Lerp(Color.red, Color.green, progress);

        elapsedTime += Time.deltaTime;
    }

    public void OnInteract()
    {
    }


    public void OnSelect()
    {
        Debug.Log($"OnSelect", this);
    }

    public void OnDeselect()
    {
        Debug.Log($"OnDeselect", this);
    }

    public void StartCrafting()
    {
        craftInfo.Value = new CraftInfo()
        {
            StartTime = NetworkManager.LocalTime.TimeAsFloat,
            CraftTime = Random.Range(10, 20)
        };
        endTime = craftInfo.Value.StartTime + craftInfo.Value.CraftTime;
    }

    public void TakeDamage(in DamageInfo damageInfo)
    {
        Debug.Log(damageInfo.ToString());
        OnTakeDamage?.Invoke();
    }
}
