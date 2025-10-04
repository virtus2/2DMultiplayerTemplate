using Unity.Netcode;
using UnityEngine;

public class PlayerCharacter : Character
{
    [Header("Player Character")]
    [SerializeField] private CharacterControlInput clientInput;

    private Player ownerPlayer;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        name = $"{nameof(PlayerCharacter)} - {OwnerClientId}";
        SetOwnerPlayer();
    }

    public void SetOwnerPlayer()
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
            return;

        if (OwnerClientId != NetworkManager.LocalClientId)
            return;

        ownerPlayer = client.PlayerObject.GetComponent<Player>();
        ownerPlayer.SetPlayerCharacter(this);
    }

    protected override void InitializeStateMachine()
    {
        stateMachine.AddState(ECharacterState.Idle, new IdleState(this, stateMachine));
        stateMachine.AddState(ECharacterState.Walk, new WalkState(this, stateMachine));
    }

    protected override void HandleInput()
    {
        if (HasAuthority)
        {
            if (IsOwner)
            {
                Input = clientInput;
            }
        }
        else
        {
            SendInputRpc(clientInput);
        }
    }

    [Rpc(SendTo.Authority)]
    private void SendInputRpc(CharacterControlInput input)
    {
        Input = input;
    }

    public void HandleMoveInput(in Vector2 moveInput)
    {
        clientInput.Move = moveInput;
    }
}
