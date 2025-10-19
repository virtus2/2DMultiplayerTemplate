using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private NetworkRigidbody2D networkRb;
    [SerializeField] private Collider2D collider2d;
    [SerializeField] private ClientProjectile projectileVisual;

    private bool initialized = false;
    private bool initializedVisual = false;

    private Vector2 direction;
    private IAttacker attacker;
    private float movementSpeed;
    private float maxRange = 10f;

    private int maxCollisions = 1;
    private int collisionCount = 0;

    private float lifeTime = 5f;
    private float destroyTime;
    private float destroyDelayWhenHit = 0f;
    private bool isDead = false;

    public void Initialize(IAttacker attacker, in Vector2 direction, float movementSpeed)
    {
        this.attacker = attacker;
        this.direction = direction;
        this.movementSpeed = movementSpeed;

        initialized = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isDead = false;
            destroyTime = Time.fixedTime + maxRange / movementSpeed;
            collisionCount = 0;
        }

        if(IsClient)
        {
            projectileVisual.Initialize(IsHost);
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {

        }

        if(IsClient)
        {
            Debug.Log($"Despawn: {rb.position}");
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (initialized && !initializedVisual)
            {
                projectileVisual.SetProjectileInfoRpc(direction, movementSpeed);
                initializedVisual = true;
            }

            if (destroyTime < Time.fixedTime)
            {
                NetworkObject.Despawn();
                return;
            }

            Vector2 targetPosition = (Vector2)transform.position + direction * movementSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (collision == null) return;
        if (isDead) return;

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
            destroyTime = Time.fixedTime + destroyDelayWhenHit;
            isDead = true;
            return;
        }
    }

    [Rpc(SendTo.NotAuthority)]
    private void TriggerEnterRpc()
    {

    }
}