using UnityEngine;

public class WalkState : IState
{
    private Character character;
    private StateMachine stateMachine;

    public WalkState(Character character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
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
        if(character.MovementVector == Vector2.zero)
        {
            stateMachine.TransitionTo(ECharacterState.Idle);
            return;
        }

        if (character.Input.Attack)
        {
            stateMachine.TransitionTo(ECharacterState.Attack);
            return;
        }
    }
}
