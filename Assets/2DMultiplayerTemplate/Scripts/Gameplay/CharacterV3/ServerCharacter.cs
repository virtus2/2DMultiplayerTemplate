using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour, IAttacker, IDamageable
{
    [Header("ReadOnly Variables")]
    [SerializeField] private Vector2Int currentSector;

    public NetworkVariable<bool> FacingRight;
    public NetworkVariable<int> EquippedWeaponIndex;

    [SerializeField] private ClientCharacter clientCharacter;

    public override void OnNetworkSpawn()
    {
        if(!HasAuthority)
        {
            enabled = false;
            return;
        }
        currentSector = new Vector2Int(int.MinValue, int.MinValue);
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

    public void AttackRangedWeapon(Vector2 position, Vector2 direction, float projectileSpeed)
    {
        // Projectile
        // Create projectile for client prediction
        ClientProjectile clientProjectile = Instantiate(GameManager.Instance.ClientProjectilePrefab, position, Quaternion.identity);
        clientProjectile.Direction = direction;
        clientProjectile.MovementSpeed = projectileSpeed;
        clientProjectile.Owner = gameObject;

        // Create projectile for server-side collision
        int ownerTick = NetworkManager.NetworkTickSystem.LocalTime.Tick;
        CreateServerProjectileRpc(position, direction, projectileSpeed, ownerTick);
    }

    [Rpc(SendTo.Server)]
    private void CreateServerProjectileRpc(Vector2 position, Vector2 direction, float projectileSpeed, int ownerTick)
    {
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
        CreateClientProjectileRpc(position, direction, projectileSpeed, ownerTick, OwnerClientId);
    }

    [Rpc(SendTo.NotAuthority)]
    private void CreateClientProjectileRpc(Vector2 position, Vector2 direction, float projectileSpeed, int ownerTick, ulong ownerClientId)
    {
        if (ownerClientId == NetworkManager.LocalClientId) return;

        NetworkTime elapsedTime = NetworkManager.LocalTime.TimeTicksAgo(ownerTick);
        float elapsedTimeAsFloat = elapsedTime.TimeAsFloat;

        Vector2 startPosition = position + (direction * projectileSpeed * elapsedTimeAsFloat);

        ClientProjectile clientProjectile = Instantiate(GameManager.Instance.ClientProjectilePrefab, startPosition, Quaternion.identity);
        clientProjectile.Direction = direction;
        clientProjectile.MovementSpeed = projectileSpeed;
        clientProjectile.Owner = gameObject;
    }

    private void UpdateSector()
    {
        Vector2Int movedSector = GameManager.Instance.GetSectorPosition(transform.position);
        if (currentSector != movedSector)
        {
            GameManager.Instance.UpdateServerSector(OwnerClientId, currentSector, movedSector);
            currentSector = movedSector;
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
