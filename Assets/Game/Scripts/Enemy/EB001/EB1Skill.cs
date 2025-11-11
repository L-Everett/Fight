using System.Collections;
using UnityEngine;

public class EB1Skill : EnemySkill
{
    //skill1
    private EffectSweepCtrl effectSweepCtrl;

    //skill2
    public GameObject skill2Prefab;
    private EffectStabCtrl effectStabCtrl;

    private bool firstSkill2 = true;
    protected override void Skill(int skillID)
    {
        if (character.currentTarget == null) return;
        #region Skill1
        if (skillID == 1)
        {
            if (firstSkill1)
            {
                if (skill1Prefab == null)
                {
                    StartCoroutine(DelaySkillEnd());
                    return;
                }
                effectSweepCtrl = GameObject.Instantiate(skill1Prefab, effectRoot.transform).GetComponent<EffectSweepCtrl>();
                firstSkill1 = false;
            }
            effectSweepCtrl.Init(character.currentTarget.position);
            effectSweepCtrl.SetCallback(() =>
            {
                if (character == null || character.currentTarget == null) return;
                var target = character.currentTarget.GetComponent<CharacterBase>();
                if (character.currentTarget != null && !target.isDead)
                {
                    //伤害倍率150%
                    float damage = character.CalculateDamage(out bool isCritical, 1.5f);
                    target.TakeDamage(damage);

                    // 显示伤害数字
                    Vector3 popupPosition = character.currentTarget.position + Vector3.up * 2f;
                    DamagePopupManager.Instance.ShowDamage(popupPosition, damage, isCritical);
                }
                StartCoroutine(DelaySkillEnd());
            });
        }
        #endregion

        #region Skill2
        else if (skillID == 2)
        {
            if (firstSkill2)
            {
                if (skill2Prefab == null)
                {
                    StartCoroutine(DelaySkillEnd());
                    return;
                }
                effectStabCtrl = GameObject.Instantiate(skill2Prefab, effectRoot.transform).GetComponent<EffectStabCtrl>();
                firstSkill2 = false;
            }
            effectStabCtrl.Init(character.currentTarget.position);
            effectStabCtrl.SetCallback(() =>
            {
                if (character == null || character.currentTarget == null) return;
                var target = character.currentTarget.GetComponent<CharacterBase>();
                if (character.currentTarget != null && !target.isDead)
                {
                    //伤害倍率180%
                    float damage = character.CalculateDamage(out bool isCritical, 1.8f);
                    target.TakeDamage(damage);

                    // 显示伤害数字
                    Vector3 popupPosition = character.currentTarget.position + Vector3.up * 2f;
                    DamagePopupManager.Instance.ShowDamage(popupPosition, damage, isCritical);
                }
                StartCoroutine(DelaySkillEnd());
            });
        }
        #endregion
    }

    protected override IEnumerator DelaySkillEnd()
    {
        return base.DelaySkillEnd();
    }
}
