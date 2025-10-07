using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour, IInteractable
{
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
}
