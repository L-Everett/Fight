using UnityEngine;

public class AttackExec : MonoBehaviour
{
    private CharacterBase characterBase;

    private void Start()
    {
        characterBase = transform.parent.GetComponent<CharacterBase>();
    }

    public void AttackOne()
    {
        var target = characterBase.currentTarget;
        if (target == null) return;
        CharacterBase targetCharacter = target.GetComponent<CharacterBase>();
        if (targetCharacter != null && !targetCharacter.isDead)
        {
            float damage = characterBase.CalculateDamage(out bool isCritical); 
            targetCharacter.TakeDamage(damage);

            // 显示伤害数字
            Vector3 popupPosition = target.position + Vector3.up * 2f;
            DamagePopupManager.Instance.ShowDamage(popupPosition, damage, isCritical);
        }
    }

    public void AttackEnd()
    {
        characterBase.isAttacking = false;
    }
}
