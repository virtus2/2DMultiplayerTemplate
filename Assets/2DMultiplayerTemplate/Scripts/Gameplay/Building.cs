using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Building : NetworkBehaviour, IInteractable, IDamageable
{
    [Header("Gameplay events")]
    public UnityEvent OnTakeDamage;

    private void Awake()
    {

    }
    public void OnInteract()
    {
    }


    public void OnSelect()
    {
        Debug.Log($"OnSelect", this);
    }

    public void OnDeselect()
    {
        Debug.Log($"OnDeselect", this);
    }

    public void TakeDamage(in DamageInfo damageInfo)
    {
        Debug.Log(damageInfo.ToString());
        OnTakeDamage?.Invoke();
    }
}
