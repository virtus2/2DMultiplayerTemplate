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
