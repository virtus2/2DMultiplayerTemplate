using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private GameObject attackerObj;
    private IAttacker attacker;

    private void Awake()
    {
        attacker = attackerObj.GetComponent<IAttacker>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            var damageInfo = attacker.GetDamageInfo(damageable);
            damageable.TakeDamage(damageInfo);
        }
    }
}
