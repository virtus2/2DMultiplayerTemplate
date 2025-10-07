using System;
using Unity.Netcode;
using UnityEngine;

public class DamageReceiver : NetworkBehaviour, IDamageable
{
    public Action OnDamageTaken;
    public Action OnDied;

    public bool IsDamageable()
    {
        return true;
    }

    public void TakeDamage(Character damageGiver, float damage)
    {
        OnDamageTaken?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackTrigger"))
        {
            var attacker = collision.GetComponentInParent<Character>();
            TakeDamageRpc(attacker.OwnerClientId, 1f);
        }
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
    private void TakeDamageRpc(ulong ownerClientId, float damage)
    {
        Debug.Log("TakeDamageRpc");
        OnDamageTaken?.Invoke();
    }
    
}