using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : NetworkBehaviour
{
    public Vector2 MoveInput = default(Vector2);

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerCharacter playerCharacter;
    private PlayerCamera playerCamera;
    

    public void OnMove(CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        if (playerCharacter)
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
        if (playerCharacter)
        {
            playerCharacter.HandleMousePosition(worldPosition);
        }
    }

    public void OnAttack(CallbackContext context)
    {
        bool attackInput = context.ReadValueAsButton();

        if (playerCharacter)
        {
            playerCharacter.HandleAttackInput(attackInput);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        name = $"{nameof(Player)} - {OwnerClientId}";
        playerInput.enabled = IsLocalPlayer;
        playerCamera = FindAnyObjectByType<PlayerCamera>();
    }

    public void SetPlayerCharacter(PlayerCharacter character)
    {
        playerCharacter = character;
        playerCamera.SetFollowTarget(character.gameObject);
    }
}
