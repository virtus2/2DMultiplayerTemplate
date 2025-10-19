using Unity.Netcode;
using UnityEngine;

public class ClientProjectile : MonoBehaviour
{
    [SerializeField] private Projectile projectile;

    [SerializeField] private Vector2 direction;
    [SerializeField] private float movementSpeed;
    private Vector2 targetPosition;

    private float lerpElapsedTime = 0f;
    private float lerpDuration = 0.1f;
    private bool useLerp = false;

    public void Initialize(bool useLerp)
    {
        lerpElapsedTime = 0f;
        transform.parent = null;
        transform.position = projectile.transform.position;
    }

    [ClientRpc]
    public void SetProjectileInfoRpc(Vector2 direction, float movementSpeed)
    {
        Debug.Log($"direction:{direction} movementSpeed:{movementSpeed}");
        this.direction = direction;
        this.movementSpeed = movementSpeed;
    }

    private void FixedUpdate()
    {
        targetPosition = (Vector2)transform.position + direction * movementSpeed * Time.fixedDeltaTime;
    }

    private void Update()
    {
        if(projectile == null)
        {
            lerpElapsedTime += Time.deltaTime;
            if (lerpElapsedTime > lerpDuration)
            {
                lerpElapsedTime = lerpDuration;
            }
            float lerpPercentage = lerpElapsedTime / lerpDuration;

            transform.position = Vector2.Lerp(transform.position, targetPosition, lerpPercentage);
        }
        else
        {
            Vector2 projectilePosition = projectile.transform.position;
            if (useLerp)
            {
                lerpElapsedTime += Time.deltaTime;
                if (lerpElapsedTime > lerpDuration)
                {
                    lerpElapsedTime = lerpDuration;
                }
                float lerpPercentage = lerpElapsedTime / lerpDuration;

                transform.position = Vector2.Lerp(transform.position, projectilePosition, lerpPercentage);
            }
            else
            {
                transform.position = projectilePosition;
            }
        }
    }
}
