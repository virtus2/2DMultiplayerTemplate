using Unity.Netcode;
using UnityEngine;

public class ServerProjectile : NetworkBehaviour
{
    public Vector2 Direction;
    public float MovementSpeed;
    public GameObject Owner;

    private void Update()
    {
        transform.position = transform.position + (Vector3)Direction * MovementSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Owner) return;
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            IAttacker attacker = Owner.GetComponent<IAttacker>();
            DamageInfo damageInfo = attacker.GetDamageInfo(damageable);
            damageable.TakeDamage(damageInfo);

            NetworkObject.Despawn();
        }
    }
}
