using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct CharacterControlInput : INetworkSerializable
{
    public Vector2 Move;
    public Vector2 Look;
    public bool Attack;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref Look);
        serializer.SerializeValue(ref Attack);
    }
}

public abstract class Character : NetworkBehaviour
{
    public CharacterControlInput Input;
    public float MoveSpeed = 1f;
    public bool IsAttacking = false;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] protected StateMachine stateMachine;
    [SerializeField] protected DamageReceiver damageReceiver;
    [SerializeField] protected Collider2D meleeAttackCollider;

    [Header("Gameplay")]
    [SerializeField] private Vector2 movementVector;
    [SerializeField] private Vector2 lookVector;

    static Lazy<int> Animator_Parameter_Hash_Velocity;
    static Lazy<int> Animator_Parameter_Hash_Right;
    static Lazy<int> Animator_Parameter_Hash_IsAttacking;

    protected abstract void InitializeStateMachine();
    protected abstract void HandleInput();

    private void Awake()
    {
        InitializeAnimatorParameterHash();
        InitializeStateMachine();
    }

    private void InitializeAnimatorParameterHash()
    {
        if (Animator_Parameter_Hash_Velocity == null) Animator_Parameter_Hash_Velocity = new Lazy<int>(Animator.StringToHash("Velocity"));
        if (Animator_Parameter_Hash_Right == null) Animator_Parameter_Hash_Right = new Lazy<int>(Animator.StringToHash("Right"));
        if (Animator_Parameter_Hash_IsAttacking == null) Animator_Parameter_Hash_IsAttacking = new Lazy<int>(Animator.StringToHash("IsAttacking"));
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

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
        lookVector = Input.Look;
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat(Animator_Parameter_Hash_Velocity.Value, MoveSpeed);
        animator.SetBool(Animator_Parameter_Hash_Right.Value, lookVector.x < 0f);
        animator.SetBool(Animator_Parameter_Hash_IsAttacking.Value, IsAttacking);
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movementVector;
        rb.MovePosition(targetPosition);
    }

    public void SetActiveAttackCollider(bool active)
    {
        meleeAttackCollider.gameObject.SetActive(active);
    }
}

