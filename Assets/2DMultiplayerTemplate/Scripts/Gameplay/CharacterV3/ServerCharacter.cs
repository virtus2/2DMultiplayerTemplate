using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour, IAttacker, IDamageable
{
    [Header("ReadOnly Variables")]
    [SerializeField] private Vector2Int currentChunk;

    public NetworkVariable<bool> FacingRight;
    public NetworkVariable<int> EquippedWeaponIndex;

    [SerializeField] private ClientCharacter clientCharacter;
    private Player ownerPlayer;

    public override void OnNetworkSpawn()
    {
        if(!HasAuthority)
        {
            enabled = false;
            return;
        }

        currentChunk = new Vector2Int(int.MinValue, int.MinValue);
        SetOwnerPlayer();
    }

    private void SetOwnerPlayer()
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
            return;

        bool isOwner = OwnerClientId == NetworkManager.LocalClientId;
        ownerPlayer = client.PlayerObject.GetComponent<Player>();
        ownerPlayer.SetPlayerCharacter(clientCharacter);
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
