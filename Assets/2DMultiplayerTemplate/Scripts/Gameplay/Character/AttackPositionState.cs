using UnityEngine;

public class AttackPositionState : IState
{
    private Character character;
    private StateMachine stateMachine;
    private float elapsedTime = 0f;
    private float duration = 0.25f;

    public AttackPositionState(Character character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    public void OnEnter()
    {
        elapsedTime = 0f;
        character.IsAttacking = true;
        if (character.IsOwner)
            character.AttackLookPosition();
    }

    public void OnExit()
    {
        elapsedTime = duration;
        character.IsAttacking = false;
    }

    public void OnServerUpdate()
    {
        float x = character.Input.Move.x * character.MoveSpeed * Time.fixedDeltaTime;
        float y = character.Input.Move.y * character.MoveSpeed * Time.fixedDeltaTime;
        character.MovementVector = new Vector2(x, y);
    }

    public void OnClientUpdate()
    {
    }

    public void CheckTransitions()
    {
        if (character.MovementVector == Vector2.zero)
        {
            stateMachine.TransitionTo(ECharacterState.Idle);
            return;
        }
        else
        {
            stateMachine.TransitionTo(ECharacterState.Walk);
            return;
        }
    }
}
