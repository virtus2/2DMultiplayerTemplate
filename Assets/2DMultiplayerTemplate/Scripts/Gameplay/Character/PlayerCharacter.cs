using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : Character, IPlayerCharacter
{
    public GameObject GameObject => gameObject;

    [Header("Player Character")]
    [SerializeField] private CharacterControlInput clientInput;

    [SerializeField] private InteractionHandler interactionHandler;


    private Player ownerPlayer;
    protected override void Awake()
    {
        base.Awake();   

        attackType = EAttackType.Position;
    }

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
        stateMachine.AddState(ECharacterState.AttackPosition, new AttackPositionState(this, stateMachine));
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
        lookPositionWorld = worldPosition;

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
        switch (attackType)
        {
            case EAttackType.Position:
                if (attackInput)
                {
                    AttackPosition();
                }
                break;
            case EAttackType.Area:
                AttackArea(attackInput);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Send attack target to server
    /// </summary>
    private void AttackPosition()
    {
        if (HasAuthority)
        {
            stateMachine.TransitionTo(ECharacterState.AttackPosition);
        }
        else
        {
            SendAttackPositionRpc();
        }
    }

    [Rpc(SendTo.Authority)]
    private void SendAttackPositionRpc()
    {
        // var damageInfo = attacker.GetDamageInfo(damageable);
        // damageable.TakeDamage(damageInfo);
        stateMachine.TransitionTo(ECharacterState.AttackPosition);
    }

    /// <summary>
    /// Send attack input to server, then server will activate attack area trigger
    /// </summary>
    private void AttackArea(bool attackInput)
    {
        clientInput.Attack = attackInput;
    }


    public void HandleInteractInput(bool interactInput)
    {

    }

    public void HandleEquipInput(bool equipInput, int index)
    {
        throw new System.NotImplementedException();
    }
}
