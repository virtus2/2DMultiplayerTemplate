using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour, IAttacker
{
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
    public void AttackRangedWeaponRpc(Vector2 direction)
    {
        GameManager.Instance.CreateProjectile(this, OwnerClientId, 10f, transform.position, Quaternion.identity, direction);
    }

    public DamageInfo GetDamageInfo(IDamageable target)
    {
        DamageInfo damageInfo = new DamageInfo()
        {
            attacker = this,
            damageAmount = 1f,
        };
        return damageInfo;
    }
}
