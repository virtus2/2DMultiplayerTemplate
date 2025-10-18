using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D collider2d;

    private Vector2 direction;
    private IAttacker attacker;
    private float movementSpeed;

    private int maxCollisions = 1;
    private int collisionCount = 0;

    private float lifeTime = 5f;
    private float elapsedTime = 0f;

    public void Initialize(IAttacker attacker, in Vector2 direction, float movementSpeed)
    {
        this.attacker = attacker;
        this.direction = direction;
        this.movementSpeed = movementSpeed;

        elapsedTime = 0f;
        collisionCount = 0;
    }

    private void FixedUpdate()
    {
        if (HasAuthority)
        {
            Vector2 targetPosition = (Vector2)transform.position + direction * movementSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            elapsedTime += Time.fixedDeltaTime;
            if(elapsedTime >= lifeTime)
            {
                NetworkObject.Despawn();
                return;
            }
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
        if (damageable == attacker) return;

        DamageInfo damageInfo = attacker.GetDamageInfo(damageable);
        damageable.TakeDamage(damageInfo);

        collisionCount++;
        if (collisionCount >= maxCollisions)
        {
            NetworkObject.Despawn();
            return;
        }
    }
}
