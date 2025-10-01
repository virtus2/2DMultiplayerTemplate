using Unity.Netcode;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private Vector2 movementVector;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        name = $"{nameof(PlayerCharacter)} - {OwnerClientId}";

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out NetworkClient client))
            return;

        if (OwnerClientId != NetworkManager.LocalClientId)
            return;

        Player player = client.PlayerObject.GetComponent<Player>();
        player.SetPlayerCharacter(this);
    }

    public void HandleMoveInput(in Vector2 moveInput)
    {
        if (HasAuthority)
        {
            Move(moveInput);
        }
        else
        {
            MoveRpc(moveInput);
        }
    }

    [Rpc(SendTo.Authority)]
    public void MoveRpc(Vector2 moveInput)
    {
        Move(moveInput);
    }

    public void Move(in Vector2 moveInput)
    {
        movementVector = new Vector2(moveInput.x * moveSpeed, moveInput.y* moveSpeed);
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movementVector;
        rb.MovePosition(targetPosition);
    }
}
