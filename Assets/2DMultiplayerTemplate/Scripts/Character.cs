using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct CharacterControlInput : INetworkSerializable
{
    public Vector2 Move;
    public bool Attack;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref Attack);
    }
}

public abstract class Character : NetworkBehaviour
{
    public CharacterControlInput Input;
    public float MoveSpeed = 1f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] protected StateMachine stateMachine;

    [SerializeField] private Vector2 movementVector;

    Lazy<int> Animator_Parameter_Hash_Velocity;

    protected abstract void InitializeStateMachine();
    protected abstract void HandleInput();

    private void Awake()
    {
        Debug.Log("AWAKE");
        InitializeAnimatorParameterHash();
        InitializeStateMachine();
    }

    private void InitializeAnimatorParameterHash()
    {

    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("OnNetworkSpawn");

        if (HasAuthority)
        {
            stateMachine.TransitionTo(ECharacterState.Idle);
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateState();
        UpdateAnimationParameters();
    }

    private void UpdateState()
    {
        stateMachine.UpdateState(ref movementVector);
    }

    private void UpdateAnimationParameters()
    {

    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movementVector;
        rb.MovePosition(targetPosition);
    }
}
