using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour, IAttacker, IDamageable
{
    public bool IsNpc => isNpc;
    public bool IsDead => isDead;
    public Vector2Int ChunkPosition => currentChunk;

    public NetworkVariable<bool> FacingRight;
    public NetworkVariable<int> EquippedWeaponIndex;

    [Header("ReadOnly Variables")]
    [SerializeField] private Vector2Int currentChunk;

    [Header("References")]
    [SerializeField] private ClientCharacter clientCharacter;

    [Header("Gameplay Variables")]
    [SerializeField] private bool isNpc;
    [SerializeField] private int aiType;


    private Player ownerPlayer;
    private AIBrain aiBrain;
    private bool isDead;

    public override void OnNetworkSpawn()
    {
        if(!HasAuthority)
        {
            enabled = false;
            return;
        }

        currentChunk = new Vector2Int(int.MinValue, int.MinValue);

        if (isNpc)
        {
            SetAI();
        }
        else
        {
            SetOwnerPlayer();
        }
    }

    private void SetOwnerPlayer()
    {
        // For server, Set character reference of Player component.
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
            return;

        bool isOwner = OwnerClientId == NetworkManager.LocalClientId;
        ownerPlayer = client.PlayerObject.GetComponent<Player>();
        ownerPlayer.SetPlayerCharacter(clientCharacter);
    }

    private void SetAI()
    {
        aiBrain = GetComponent<AIBrain>();
    }

    private void Update()
    {
        if(isNpc && aiBrain)
            aiBrain.UpdateAI();
    }

    private void FixedUpdate()
    {
        UpdateSector();
    }

    [Rpc(SendTo.Server)]
    public void SetFacingRpc(bool facingRight)
    {
        FacingRight.Value = facingRight;
    }

    [Rpc(SendTo.Server)]
    public void EquipWeaponRpc(int index)
    {
        EquippedWeaponIndex.Value = index;
    }

    [Rpc(SendTo.Server)]
    public void CreateServerProjectileRpc(Vector2 position, Vector2 direction, float projectileSpeed, int ownerTick)
    {
        // Create projectile for server collision (ServerCharacter.CreateServerProjectileRpc)
        NetworkTime elapsedTime = NetworkManager.LocalTime.TimeTicksAgo(ownerTick);
        float elapsedTimeAsFloat = elapsedTime.TimeAsFloat;
        
        Vector2 startPosition = position + (direction * projectileSpeed * elapsedTimeAsFloat);

        NetworkObject obj = GameManager.Instance.ServerProjectilePrefab.InstantiateAndSpawn(NetworkManager,
            ownerClientId: OwnerClientId,
            position: startPosition,
            rotation: Quaternion.identity
        );

        ServerProjectile serverProjectile = obj.GetComponent<ServerProjectile>();
        serverProjectile.Direction = direction;
        serverProjectile.MovementSpeed = projectileSpeed;
        serverProjectile.Owner = gameObject;

        // Create projectile for other clients (except shooter client)
        clientCharacter.CreateClientProjectileRpc(position, direction, projectileSpeed, ownerTick, OwnerClientId);
    }

    private void UpdateSector()
    {
        Vector2Int movedChunk = ServerChunkLoader.GetChunkPosition(transform.position);
        if (currentChunk != movedChunk)
        {
            GameManager.Instance.ServerChunkLoader.UpdateChunk(OwnerClientId, currentChunk, movedChunk);
            currentChunk = movedChunk;
        }
    }

    #region IDamageable
    public DamageInfo GetDamageInfo(IDamageable target)
    {
        DamageInfo damageInfo = new DamageInfo()
        {
            attacker = this,
            damageAmount = 1f,
        };
        return damageInfo;
    }

    public void TakeDamage(in DamageInfo damageInfo)
    {
        Debug.Log($"TakeDamage {damageInfo.damageAmount}");
    }
    #endregion
}
