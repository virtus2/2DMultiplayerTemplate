using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ECharacterState
{
    Idle,
    Walk,
}

public class StateMachine : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<ECharacterState> currentState;


    private Dictionary<ECharacterState, IState> states = new Dictionary<ECharacterState, IState>();
    private IState currentStateInstance;

    public void AddState(ECharacterState state, IState stateInstance)
    {
        states.Add(state, stateInstance);
    }

    public void UpdateState(ref Vector2 movementVector)
    {
        currentStateInstance?.OnUpdate(ref movementVector);
    }

    public void TransitionTo(ECharacterState state)
    {
        if (HasAuthority)
        {
            TransitionToInternal(state);
        }
        else
        {
            RequestTransitionToRpc(state);
        }
    }

    [Rpc(SendTo.Authority)]
    private void RequestTransitionToRpc(ECharacterState state)
    {
        TransitionToInternal(state);
    }

    private void TransitionToInternal(ECharacterState state)
    {
        currentStateInstance?.OnExit();
        currentState.Value = state;
        currentStateInstance = states[state];
        currentStateInstance.OnEnter();
    }
}
