using UnityEngine;

public interface IPlayerCharacter 
{
    public GameObject GameObject { get; }

    public void HandleMoveInput(in Vector2 moveInput);
    public void HandleAttackInput(bool attackInput);
    public void HandleInteractInput(bool interactInput);
    public void HandleEquipInput(bool equipInput, int index);

    public void HandleMousePosition(in Vector3 worldPosition);
}
