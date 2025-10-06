using UnityEngine;

public class AttackState : IState
{
    private Character character;
    private StateMachine stateMachine;
    private float elapsedTime = 0f;
    private float duration = 1f;

    public AttackState(Character character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    public void OnEnter()
    {
        elapsedTime = 0f;
        character.IsAttacking = true;
        character.SetActiveAttackCollider(true);
    }

    public void OnExit()
    {
        elapsedTime = duration;
        character.IsAttacking = false;
        character.SetActiveAttackCollider(false);
    }

    public void OnUpdate(ref Vector2 movementVector)
    {
        float x = character.Input.Move.x * character.MoveSpeed * Time.fixedDeltaTime;
        float y = character.Input.Move.y * character.MoveSpeed * Time.fixedDeltaTime;
        movementVector = new Vector2(x, y);

        if (elapsedTime >= duration)
        {
            if (movementVector == Vector2.zero)
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

        elapsedTime += Time.deltaTime;
    }
}
