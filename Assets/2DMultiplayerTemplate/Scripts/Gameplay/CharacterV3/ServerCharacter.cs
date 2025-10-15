using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour
{
    public NetworkVariable<bool> FacingRight;

    [SerializeField] private ClientCharacter clientCharacter;

    public override void OnNetworkSpawn()
    {
        if(!HasAuthority)
        {
            enabled = false;
            return;
        }
    }

    [Rpc(SendTo.Server)]
    public void SetFacingRpc(bool facingRight)
    {
        FacingRight.Value = facingRight;
    }
}
