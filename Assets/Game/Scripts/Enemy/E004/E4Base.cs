using UnityEngine;

public class E4Base : EnemyBase
{
    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        hpCtrl.FreshHp(currentHp, maxHp);

        animator.SetTrigger("Hurt");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected override void UpdateAnimationSpeed()
    {
        
    }
}
