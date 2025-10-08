[System.Serializable]
public struct DamageInfo
{
    public float damageAmount;
    public IAttacker attacker;

    public DamageInfo(float damageAmount, IAttacker attacker)
    {
        this.damageAmount = damageAmount;
        this.attacker = attacker;
    }

    public override string ToString()
    {
        return $"Damage:{damageAmount}, Attacker:{attacker}";
    }
}