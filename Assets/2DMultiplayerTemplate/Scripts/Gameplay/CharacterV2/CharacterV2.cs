using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

[System.Serializable]
public struct InputPayload : INetworkSerializable
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

public abstract class CharacterV2 : NetworkBehaviour, IDamageable, IAttacker
{
    public CharacterControlInput Input;
    public float MoveSpeed = 3f;
    public Vector2 MovementVector;
    public bool IsAttacking = false;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] protected Collider2D meleeAttackCollider;

    [Header("Gameplay values")]
    [SerializeField] protected EAttackType attackType;
    [SerializeField] private Vector2 lookVector;
    [SerializeField] protected Vector3 lookPositionWorld;
    [SerializeField] private float maxHealthPoint = 3f;
    [SerializeField] private float curHealthPoint = 3f;
    [SerializeField] private float attackCooldownTime = 0.25f;
    [SerializeField] private float damage = 1f;

    [Header("Gameplay events")]
    public UnityEvent OnTakeDamage;

    static Lazy<int> Animator_Parameter_Hash_Velocity;
    static Lazy<int> Animator_Parameter_Hash_Right;
    static Lazy<int> Animator_Parameter_Hash_IsAttacking;

    protected abstract void HandleInput();

    protected virtual void Awake()
    {
        InitializeAnimatorParameterHash();
        InitializeStatus();
    }

    private void InitializeAnimatorParameterHash()
    {
        if (Animator_Parameter_Hash_Velocity == null) Animator_Parameter_Hash_Velocity = new Lazy<int>(Animator.StringToHash("Velocity"));
        if (Animator_Parameter_Hash_Right == null) Animator_Parameter_Hash_Right = new Lazy<int>(Animator.StringToHash("Right"));
        if (Animator_Parameter_Hash_IsAttacking == null) Animator_Parameter_Hash_IsAttacking = new Lazy<int>(Animator.StringToHash("IsAttacking"));
    }
    
    private void InitializeStatus()
    {
        // health point
        curHealthPoint = maxHealthPoint;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        HandleInput();
        UpdateVelocity();
        UpdateAnimationParameters();
    }

    private void UpdateVelocity()
    {
        float x = Input.Move.x * MoveSpeed * Time.deltaTime;
        float y = Input.Move.y * MoveSpeed * Time.deltaTime;
        MovementVector = new Vector2(x, y);
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat(Animator_Parameter_Hash_Velocity.Value, MoveSpeed);
        animator.SetBool(Animator_Parameter_Hash_Right.Value, lookVector.x < 0f);
        animator.SetBool(Animator_Parameter_Hash_IsAttacking.Value, IsAttacking);
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + MovementVector;
        rb.MovePosition(targetPosition);
    }

    #region IDamageable
    public void TakeDamage(in DamageInfo damageInfo)
    {
        OnTakeDamage?.Invoke();
    }
    #endregion

    #region IAttacker
    public DamageInfo GetDamageInfo(IDamageable target)
    {
        var damageInfo = new DamageInfo()
        {
            damageAmount = damage,
            attacker = this
        };
        return damageInfo;
    }
    #endregion
}

