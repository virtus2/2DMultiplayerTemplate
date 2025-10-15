using UnityEngine;

public class ClientCharacterWeapon : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void HandleFacingFlip(bool facingRight)
    {
        int facingParam = AnimatorHashCache.GetParameterHash(AnimatorHashCache.Parameter_Weapon_FacingRight);
        animator.SetBool(facingParam, facingRight);
    }

    public void HandleAttack()
    {
        int attackParam = AnimatorHashCache.GetParameterHash(AnimatorHashCache.Parameter_Weapon_Attack);
        animator.SetTrigger(attackParam);
    }
}
