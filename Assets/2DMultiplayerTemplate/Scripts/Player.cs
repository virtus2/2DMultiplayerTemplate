using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : NetworkBehaviour
{
    public Vector2 MoveInput = default(Vector2);

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private IPlayerCharacter playerCharacter;
    private PlayerCamera playerCamera;

    public void OnMove(CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        if (playerCharacter != null)
        {
            playerCharacter.HandleMoveInput(moveInput);
        }
    }

    public void OnLook(CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        float distanceFromCamera = 10f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, distanceFromCamera));

        if (playerCharacter != null)
        {
            playerCharacter.HandleMousePosition(worldPosition);
        }
    }

    public void OnAttack(CallbackContext context)
    {
        bool attackInput = context.ReadValueAsButton();

        if (playerCharacter != null)
        {
            playerCharacter.HandleAttackInput(attackInput);
        }
    }

    public void OnInteract(CallbackContext context)
    {
        bool interactInput = context.ReadValueAsButton();

        if (playerCharacter != null)
        {
            playerCharacter.HandleInteractInput(interactInput);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        name = $"{nameof(Player)} - {OwnerClientId}";
        playerInput.enabled = IsLocalPlayer;
        playerCamera = FindAnyObjectByType<PlayerCamera>();
    }

    public void SetPlayerCharacter(IPlayerCharacter character)
    {
        playerCharacter = character;
        if (IsOwner)
        {
            playerCamera.SetFollowTarget(character.GameObject);
        }
    }
}
