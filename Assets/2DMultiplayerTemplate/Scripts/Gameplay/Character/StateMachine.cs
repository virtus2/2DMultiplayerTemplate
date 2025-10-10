using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ECharacterState
{
    Idle,
    Walk,
    Attack,
    AttackPosition,
}

public class StateMachine : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<ECharacterState> currentState;

    private Dictionary<ECharacterState, IState> states = new Dictionary<ECharacterState, IState>();
    private IState currentStateInstance;

    private void OnEnable()
    {
        currentState.OnValueChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        currentState.OnValueChanged -= HandleStateChanged;
    }

    public void AddState(ECharacterState state, IState stateInstance)
    {
        states.Add(state, stateInstance);
    }

    public void UpdateState()
    {
        if (HasAuthority)
        {
            currentStateInstance?.OnServerUpdate();
        }

        currentStateInstance?.OnClientUpdate();

        if (HasAuthority)
        {
            currentStateInstance?.CheckTransitions();
        }
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
        currentState.Value = state;
    }

    private void HandleStateChanged(ECharacterState prevState, ECharacterState nextState)
    {
        currentStateInstance?.OnExit();
        currentStateInstance = states[nextState];
        currentStateInstance.OnEnter();
    }
}
