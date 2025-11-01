using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBase : CharacterBase
{
    [Header("Enemy Settings")]
    public Transform characterRoot; // 玩家角色根节点
    public string playerTag = "Player"; // 玩家标签

    protected override void Awake()
    {
        base.Awake();

        // 自动查找玩家角色根节点
        if (characterRoot == null)
        {
            GameObject rootObj = GameObject.Find("CharacterRoot");
            if (rootObj != null) characterRoot = rootObj.transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        SkillCool();
    }

    public override void Init()
    {
        battleManager = BattleManager.Instance;
        // 初始化属性
        if (id == string.Empty)
        {
            attributes.Init("E001", true);
        }
        else
        {
            attributes.Init(id, true);
            //初始化技能
            coolTimes.Clear();
            coolTimers.Clear();
            var data = StaticDataInterface.Enemy.GetEnemy(id);
            var skillIDList = data.Skills;
            var skillCools = data.SkillCools;
            for (int i = 0; i < skillIDList.Count; i++)
            {
                coolTimes.Add(skillIDList[i], skillCools[i]);
                coolTimers.Add(skillIDList[i], 0);
            }
            skillKeys = new List<int>(coolTimes.Keys);
        }

        // 设置初始生命值
        float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        currentHp = maxHp;
        hpCtrl.FreshHp(currentHp, maxHp);

        // 重置状态
        isDead = false;
        isAttacking = false;
        isMoving = false;
        currentTarget = null;
        lastAttackTime = 0;
        lastAcquireTime = 0;

        // 更新动画速度
        UpdateAnimationSpeed();
    }

    // 重写索敌方法 - 针对玩家角色
    public override void AcquireTarget()
    {
        // 如果当前目标有效且距离合适，保持当前目标
        if (currentTarget != null) return;

        // 重置当前目标
        currentTarget = null;
        float closestSqrDistance = float.MaxValue;

        // 遍历CharacterRoot下的所有玩家角色
        if (characterRoot != null)
        {
            foreach (Transform player in characterRoot)
            {
                // 检查标签是否为玩家
                if (!player.CompareTag(playerTag)) continue;

                CharacterBase playerCharacter = player.GetComponent<CharacterBase>();
                if (playerCharacter == null || playerCharacter.isDead) continue;

                float sqrDistance = (transform.position - player.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    currentTarget = player;
                }
            }
        }
    }

    //释放技能AI接口
    public void ReleaseSkill(int skillID)
    {
        EnemySkill(skillID);
    }

    // 敌人技能
    protected Dictionary<int, float> coolTimes = new Dictionary<int, float>();
    private Dictionary<int, float> coolTimers = new Dictionary<int, float>();
    protected virtual void EnemySkill(int skillID)
    {
        animator.SetTrigger($"Skill{skillID}");
    }

    //技能冷却
    private List<int> skillKeys;
    void SkillCool()
    {
        foreach(var key in skillKeys)
        {
            coolTimers[key] += Time.deltaTime * battleManager.currentTimeSpeed;
        }
    }

    bool GetSkillCoolDown(int skillID)
    {
        if(coolTimes.ContainsKey(skillID) && coolTimers.ContainsKey(skillID))
        {
            if (coolTimers[skillID] >= coolTimes[skillID])
            {
                coolTimers[skillID] = 0;
                return true;
            }
        }
        return false;
    }
}