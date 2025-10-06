using UnityEngine;

public class IdleState : IState
{
    private Character character;
    private StateMachine stateMachine;
    
    public IdleState(Character character, StateMachine stateMachine)
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

    public void OnUpdate(ref Vector2 movementVector)
    {
        if (character.Input.Move != Vector2.zero)
        {
            stateMachine.TransitionTo(ECharacterState.Walk);
            return;
        }

        if (character.Input.Attack)
        {
            stateMachine.TransitionTo(ECharacterState.Attack);
            return;
        }
    }
}
