using UnityEngine;

public interface IAttacker
{
    public DamageInfo GetDamageInfo(IDamageable target);
}