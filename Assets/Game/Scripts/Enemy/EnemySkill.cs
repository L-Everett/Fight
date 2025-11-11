using System.Collections;
using UnityEngine;

public class EnemySkill : MonoBehaviour
{
    protected Transform effectRoot;
    protected CharacterBase character;

    //skill1
    protected EffectShockCtrl effectShockCtrl;
    public GameObject skill1Prefab;

    // Start is called before the first frame update
    void Start()
    {
        effectRoot = GameObject.Find("EffectRoot").transform;
        character = GetComponent<CharacterBase>();
    }

    public void RunSelectSkill(int skillID)
    {
        Skill(skillID);
    }

    //地龙通用技能
    protected bool firstSkill1 = true;
    protected virtual void Skill(int skillID)
    {
        if(character.currentTarget == null) return;
        if(skillID == 1)
        {
            if (firstSkill1)
            {
                if (skill1Prefab == null)
                {
                    StartCoroutine(DelaySkillEnd());
                    return;
                }
                effectShockCtrl = GameObject.Instantiate(skill1Prefab, effectRoot.transform).GetComponent<EffectShockCtrl>();
                firstSkill1 = false;
            }
            effectShockCtrl.Init(character.currentTarget.position);
            effectShockCtrl.SetCallback(() =>
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
    }

    //技能后摇
    protected virtual IEnumerator DelaySkillEnd()
    {
        float time = 0.3f;
        if (BattleManager.Instance.currentTimeSpeed == 2) time = 0.15f;
        yield return new WaitForSeconds(time);
        character.isAttacking = false;
        character.isSkill = false;
    }
}
