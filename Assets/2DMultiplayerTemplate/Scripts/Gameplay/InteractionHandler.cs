using Unity.VisualScripting;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    private IInteractable currInteractable;

    public void HandleMousePosition(in Vector3 worldPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        IInteractable interactable = null;

        if (hit.collider != null)
        {
            interactable = hit.collider.GetComponent<IInteractable>();
        }

        if (interactable != null && interactable != currInteractable)
        {
            if (currInteractable != null)
            {
                currInteractable.OnDeselect();
            }

            interactable.OnSelect();
            currInteractable = interactable;
        }
        else if(interactable == null && currInteractable != null)
        {
            currInteractable.OnDeselect();
            currInteractable = null;
        }
    }
}
