using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D collider2d;

    private Vector2 direction;
    private IAttacker attacker;
    private float movementSpeed;

    public override void OnNetworkSpawn()
    {

    }

    public void Initialize(IAttacker attacker, in Vector2 direction, float movementSpeed)
    {
        this.attacker = attacker;
        this.direction = direction;
        this.movementSpeed = movementSpeed;
    }

    private void FixedUpdate()
    {
        if (HasAuthority)
        {
            Vector2 targetPosition = (Vector2)transform.position + direction * movementSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!HasAuthority) return;
        if (collision == null) return;

        if (attacker == null)
        {
            // TODO: Handle projectile when attacker is already destroyed. 
            NetworkObject.Despawn();
            return;
        }

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null) return;

        DamageInfo damageInfo = attacker.GetDamageInfo(damageable);
        damageable.TakeDamage(damageInfo);
    }
}
