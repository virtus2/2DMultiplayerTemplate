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

    private void Awake()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (HasAuthority)
        {
            NetworkObject.CheckObjectVisibility += CheckObjectVisibility;
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
        }
        else
        {
            craftInfo.OnValueChanged -= OnCraftInfoChanged;
        }
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

    public bool CheckObjectVisibility(ulong clientId)
    {
        if (!IsSpawned) return false;
        if (!NetworkManager.ConnectedClients[clientId].PlayerObject.TryGetComponent<Player>(out var player)) return false;
        if (!player.TryGetPlayerCharacter(out var playerCharacter)) return false;

        Vector2Int characterSectorPosition = GameManager.Instance.GetSectorPosition(playerCharacter.GameObject.transform.position);
        Vector2Int sectorPosition = GameManager.Instance.GetSectorPosition(transform.position);
        int distanceX = Mathf.Abs(characterSectorPosition.x - sectorPosition.x);
        int distanceY = Mathf.Abs(characterSectorPosition.y - sectorPosition.y);
        return distanceX >= 0 && distanceX < 2 && distanceY >= 0 && distanceY < 2;
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
