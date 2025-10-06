using System;
using Unity.Netcode;
using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
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
            TakeDamage(attacker, 1f);
        }
    }
}