using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [HideInInspector] public EnemyBase enemy;
    public float decisionInterval = 0.3f;
    private BattleManager battleManager;

    // 技能列表
    private List<int> skillIDList = new List<int>();

    // FSM状态枚举
    private enum AIState
    {
        Idle,       // 待机
        Move,      // 移动
        Attack,     // 普通攻击
        Skill       // 释放技能
    }

    private AIState currentState = AIState.Idle;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        if (enemy == null) enemy = GetComponent<EnemyBase>();

        // 从配置加载技能列表
        LoadSkillList();

        StartCoroutine(AIDecisionRoutine());
    }

    private void LoadSkillList()
    {
        // 从静态数据加载技能列表
        var data = StaticDataInterface.Enemy.GetEnemy(enemy.GetID());
        if (data != null && data.Skills != null)
        {
            skillIDList = data.Skills;
        }
    }

    private IEnumerator AIDecisionRoutine()
    {
        while (true)
        {
            if (!enemy.isDead && battleManager.currentStage == BattleStage.Battle)
            {
                // 检查是否有存活的玩家
                bool hasAlivePlayers = CheckForAlivePlayers();

                if (!hasAlivePlayers)
                {
                    // 没有玩家时待机不动
                    currentState = AIState.Idle;
                    enemy.StopMoving();
                }
                else
                {
                    // 有存活的玩家，执行AI决策
                    ExecuteAIState();
                }
            }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private void ExecuteAIState()
    {
        // 确保有目标
        if (enemy.currentTarget == null)
        {
            enemy.AcquireTarget();
            if (enemy.currentTarget == null)
            {
                currentState = AIState.Idle;
                enemy.StopMoving();
                return;
            }
        }

        // 计算到目标的距离
        float attackRange = enemy.attributes.GetFinalAttr(
            AttributeType.AttackRange, enemy.attributeModifiers);

        float sqrDistance = (transform.position - enemy.currentTarget.position).sqrMagnitude;
        float sqrAttackRange = attackRange * attackRange;

        // 状态转换逻辑
        switch (currentState)
        {
            case AIState.Idle:
                // 从待机转换到其他状态
                if (sqrDistance <= sqrAttackRange)
                {
                    // 在攻击范围内，优先检查技能
                    int availableSkill = GetAvailableSkill();
                    if (availableSkill != -1)
                    {
                        currentState = AIState.Skill;
                    }
                    else
                    {
                        currentState = AIState.Attack;
                    }
                }
                else
                {
                    currentState = AIState.Move;
                }
                break;

            case AIState.Move:
                // 追击中检查状态转换
                if (sqrDistance <= sqrAttackRange)
                {
                    int availableSkill = GetAvailableSkill();
                    if (availableSkill != -1)
                    {
                        currentState = AIState.Skill;
                    }
                    else
                    {
                        currentState = AIState.Attack;
                    }
                }
                // 否则保持追击状态
                break;

            case AIState.Attack:
                // 攻击中检查状态转换
                if (sqrDistance > sqrAttackRange)
                {
                    currentState = AIState.Move;
                }
                else
                {
                    // 仍在攻击范围内，检查是否有技能可用
                    int availableSkill = GetAvailableSkill();
                    if (availableSkill != -1)
                    {
                        currentState = AIState.Skill;
                    }
                }
                break;

            case AIState.Skill:
                // 技能释放后回到待机，重新评估
                currentState = AIState.Idle;
                break;
        }

        // 执行当前状态的行为
        switch (currentState)
        {
            case AIState.Idle:
                enemy.StopMoving();
                break;

            case AIState.Move:
                Vector3 direction = (enemy.currentTarget.position - transform.position).normalized;
                enemy.Move(direction);
                break;

            case AIState.Attack:
                enemy.StopMoving();
                if (!enemy.isAttacking && Time.time - enemy.lastAttackTime > 0.1f)
                {
                    enemy.AttackCommand();
                }
                break;

            case AIState.Skill:
                enemy.StopMoving();
                int skillToUse = GetAvailableSkill();
                if (skillToUse != -1)
                {
                    enemy.ReleaseSkill(skillToUse);
                }
                break;
        }
    }

    // 获取第一个可用的技能ID
    private int GetAvailableSkill()
    {
        foreach (int skillID in skillIDList)
        {
            if (enemy.GetSkillCoolDown(skillID))
            {
                return skillID;
            }
        }
        return -1; // 没有可用技能
    }

    private bool CheckForAlivePlayers()
    {
        if (enemy.characterRoot == null) return false;

        foreach (Transform player in enemy.characterRoot)
        {
            if (!player.CompareTag(enemy.playerTag)) continue;

            CharacterBase playerChar = player.GetComponent<CharacterBase>();
            if (playerChar != null && !playerChar.isDead)
            {
                return true;
            }
        }
        return false;
    }
}