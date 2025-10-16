using UnityEngine;

public class ClientCharacterWeapon : MonoBehaviour
{
    [Header("References")]
    [Header("Character")]
    [SerializeField] private SpriteRenderer characterSpriteRenderer;

    [Header("Ranged")]
    [SerializeField] private GameObject rangedWeaponParent;
    [SerializeField] private Animator rangedWeaponAnimator;
    [SerializeField] private SpriteRenderer rangedWeaponSpriteRenderer;
    [SerializeField] private ClientNetworkAnimator rangedWeaponClientNetworkAnimator;

    [Header("Melee")]
    [SerializeField] private GameObject meleeWeaponParent;
    [SerializeField] private Animator meleeWeaponAnimator;
    [SerializeField] private SpriteRenderer meleeWeaponSpriteRenderer;
    [SerializeField] private ClientNetworkAnimator meleeWeaponClientNetworkAnimator;


    private int currentEquippedType = 0;

    public void HandleCursorPosition(Vector3 position)
    {
        if (currentEquippedType == 1)
        {

            Vector2 direction = (position - transform.position).normalized;
            rangedWeaponParent.transform.right = direction;

            Vector2 scale = transform.localScale;
            if (direction.x < 0)
            {
                scale.y = -1;

            }
            else if (direction.x > 0)
            {
                scale.y = 1;
            }

            rangedWeaponParent.transform.localScale = scale;
            float zAngle = rangedWeaponParent.transform.eulerAngles.z;
            if (zAngle > 0 && zAngle < 180)
            {
                rangedWeaponSpriteRenderer.sortingOrder = characterSpriteRenderer.sortingOrder - 1;
            }
            else
            {
                rangedWeaponSpriteRenderer.sortingOrder = characterSpriteRenderer.sortingOrder + 1;
            }
        }
    }

    public void HandleFacingFlip(bool facingRight)
    {
        int facingParam = AnimatorHashCache.GetParameterHash(AnimatorHashCache.Parameter_Weapon_FacingRight);
        meleeWeaponAnimator.SetBool(facingParam, facingRight);
    }

    public void HandleAttack()
    {
        int attackParam = AnimatorHashCache.GetParameterHash(AnimatorHashCache.Parameter_Weapon_Attack);
        meleeWeaponAnimator.SetTrigger(attackParam);
    }

    public void HandleEquipWeapon(int type)
    {
        currentEquippedType = type;

        if (type == 0)
        {
            meleeWeaponSpriteRenderer.enabled = true;
            meleeWeaponAnimator.enabled = true;
            meleeWeaponClientNetworkAnimator.enabled = true;

            rangedWeaponAnimator.enabled = false;
            rangedWeaponSpriteRenderer.enabled = false;
            rangedWeaponClientNetworkAnimator.enabled = false;
        }
        else if (type == 1)
        {
            meleeWeaponSpriteRenderer.enabled = false;
            meleeWeaponAnimator.enabled = false;
            meleeWeaponClientNetworkAnimator.enabled = false;

            rangedWeaponAnimator.enabled = true;
            rangedWeaponSpriteRenderer.enabled = true;
            rangedWeaponClientNetworkAnimator.enabled = true;
        }
    }
}
