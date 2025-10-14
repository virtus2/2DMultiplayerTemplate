using Unity.Netcode;
using UnityEngine;

public class ServerCharacter : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if(!HasAuthority)
        {
            enabled = false;
            return;
        }


    }
}
