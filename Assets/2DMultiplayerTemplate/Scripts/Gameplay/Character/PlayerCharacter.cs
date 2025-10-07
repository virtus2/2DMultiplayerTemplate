using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : Character
{
    [Header("Player Character")]
    [SerializeField] private CharacterControlInput clientInput;

    [SerializeField] private InteractionHandler interactionHandler;

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

        bool isOwner = OwnerClientId == NetworkManager.LocalClientId;
        ownerPlayer = client.PlayerObject.GetComponent<Player>();
        ownerPlayer.SetPlayerCharacter(this);

        interactionHandler.enabled = isOwner;
    }

    protected override void InitializeStateMachine()
    {
        stateMachine.AddState(ECharacterState.Idle, new IdleState(this, stateMachine));
        stateMachine.AddState(ECharacterState.Walk, new WalkState(this, stateMachine));
        stateMachine.AddState(ECharacterState.Attack, new AttackState(this, stateMachine));
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

    public void HandleMousePosition(in Vector3 worldPosition)
    {
        Vector3 lookVector = worldPosition - transform.position;
        lookVector.Normalize();
        clientInput.Look = new Vector2(lookVector.x, lookVector.y);

        if (interactionHandler.enabled)
        {
            interactionHandler.HandleMousePosition(worldPosition);
        }
    }

    public void HandleAttackInput(bool attackInput)
    {
        clientInput.Attack = attackInput;
    }

    public void HandleInteractInput(bool interactInput)
    {

    }
}
