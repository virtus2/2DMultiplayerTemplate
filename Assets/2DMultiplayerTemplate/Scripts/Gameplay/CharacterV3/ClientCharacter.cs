using Unity.Netcode;
using UnityEngine;

public class ClientCharacter : NetworkBehaviour, IPlayerCharacter
{
    [Header("ReadOnly Variables")]
    [SerializeField] private CharacterControlInput input;
    [SerializeField] private Vector3 cursorPosition;
    [SerializeField] private Vector2 movementVector;
    [SerializeField] private Vector2 facingVector;
    [SerializeField] private int equippedWeaponType = 0;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ServerCharacter serverCharacter;
    [SerializeField] private InteractionHandler interactionHandler;
    [SerializeField] private ClientCharacterWeapon clientCharacterWeapon;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Gameplay Variables")]
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float attackSpeed = 0.5f;
    [SerializeField] private float projectileSpeed = 10f;
    
    private Player ownerPlayer;
    private float attackCooldownTime = 0f;

    public override void OnNetworkSpawn()
    {
        interactionHandler.enabled = IsOwner;

        SetOwnerPlayer();

        serverCharacter.FacingRight.OnValueChanged += HandleServerFacingRightFlip;
        serverCharacter.EquippedWeaponIndex.OnValueChanged += HandleEquipWeapon;
    }

    protected override void OnNetworkPostSpawn()
    {
        if (IsOwner)
        {
            HandleEquipInput(true, 0);
        }
        else
        {
            HandleEquipWeapon(-1, serverCharacter.EquippedWeaponIndex.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        serverCharacter.FacingRight.OnValueChanged -= HandleServerFacingRightFlip;
        serverCharacter.EquippedWeaponIndex.OnValueChanged -= HandleEquipWeapon;
    }

    private void SetOwnerPlayer()
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
            return;

        bool isOwner = OwnerClientId == NetworkManager.LocalClientId;
        ownerPlayer = client.PlayerObject.GetComponent<Player>();
        ownerPlayer.SetPlayerCharacter(this);
    }

    private void Update()
    {
        if(IsOwner)
        {
            UpdateFacing();
            UpdateMovement();
            UpdateAttack();
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movementVector * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        facingVector = movementVector.normalized;

        clientCharacterWeapon.HandleCursorPosition(cursorPosition);
    }

    private void UpdateFacing()
    {
        bool facingRight = serverCharacter.FacingRight.Value;
        if (facingVector.x < 0f && facingRight)
        {
            FacingRightFlip(false);
            serverCharacter.SetFacingRpc(false);
        }
        else if(facingVector.x > 0 && !facingRight)
        {
            FacingRightFlip(true);
            serverCharacter.SetFacingRpc(true);
        }
    }

    private void HandleServerFacingRightFlip(bool prevFacingRight, bool currFacingRight)
    {
        // Called on simulated proxy
        Debug.Log($"HandleServerFacingRightFlip: ownerClientId({OwnerClientId}), name({name})");
        spriteRenderer.flipX = !currFacingRight;
        clientCharacterWeapon.HandleFacingFlip(currFacingRight);
    }

    private void FacingRightFlip(bool flip)
    {
        spriteRenderer.flipX = !flip;
        clientCharacterWeapon.HandleFacingFlip(flip);
    }

    private void HandleEquipWeapon(int prevIndex, int currIndex)
    {
        int equippedType = -1;
        if (currIndex == 0)
        {
            equippedType = 0;
        }
        else if (currIndex == 1)
        {
            equippedType = 1;
        }

        clientCharacterWeapon.HandleEquipWeapon(equippedType);
    }

    private void UpdateMovement()
    {
        movementVector = new Vector2(input.Move.x * movementSpeed, input.Move.y * movementSpeed);
    }

    private void UpdateAttack()
    {
        if (input.Attack)
        {
            if (attackCooldownTime <= 0f)
            {
                attackCooldownTime = attackSpeed;
                TryAttack();
            }
        }

        attackCooldownTime -= Time.deltaTime;
    }

    private void TryAttack()
    {
        if (equippedWeaponType == 0)
        {

        }
        else if (equippedWeaponType == 1)
        {
            Vector2 direction = cursorPosition - transform.position;
            serverCharacter.AttackRangedWeapon(transform.position, direction.normalized, projectileSpeed);
        }
        clientCharacterWeapon.HandleAttack();
    }


    #region IPlayerCharacter
    public GameObject GameObject => gameObject;

    public void HandleAttackInput(bool attackInput)
    {
        input.Attack = attackInput;
    }

    public void HandleInteractInput(bool interactInput)
    {

    }

    public void HandleMousePosition(in Vector3 worldPosition)
    {
        cursorPosition = worldPosition;
    }

    public void HandleMoveInput(in Vector2 moveInput)
    {
        input.Move = moveInput;
    }

    public void HandleEquipInput(bool equipInput, int index)
    {
        // TODO: Implement equipment hot bar
        if (index == 0)
        {
            equippedWeaponType = 0;
        }
        else if (index == 1)
        {
            equippedWeaponType = 1;
        }

        serverCharacter.EquipWeaponRpc(index);
        clientCharacterWeapon.HandleEquipWeapon(equippedWeaponType);
    }
    #endregion
}
