using Unity.Netcode;
using UnityEngine;

public class ClientCharacter : NetworkBehaviour, IPlayerCharacter
{
    [Header("ReadOnly Variables")]
    [SerializeField] private CharacterControlInput input;
    [SerializeField] private Vector2 movementVector;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InteractionHandler interactionHandler;

    [Header("Gameplay Variables")]
    [SerializeField] private float movementSpeed = 1f;

    private Player ownerPlayer;

    public override void OnNetworkSpawn()
    {
        interactionHandler.enabled = IsOwner;

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        SetOwnerPlayer();
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
        movementVector = new Vector2(input.Move.x * movementSpeed, input.Move.y * movementSpeed);
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movementVector * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
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

    }

    public void HandleMoveInput(in Vector2 moveInput)
    {
        input.Move = moveInput;
    }
    #endregion
}
