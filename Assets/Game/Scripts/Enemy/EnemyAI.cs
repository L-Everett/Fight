using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyBase enemy;
    public float decisionInterval = 0.3f;
    private BattleManager battleManager;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        if (enemy == null) enemy = GetComponent<EnemyBase>();
        StartCoroutine(AIDecisionRoutine());
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
                    enemy.StopMoving();
                }
                else if (enemy.currentTarget != null)
                {
                    // 计算到目标的距离
                    float attackRange = enemy.attributes.GetFinalAttr(
                        AttributeType.AttackRange, enemy.attributeModifiers);

                    float sqrDistance = (transform.position - enemy.currentTarget.position).sqrMagnitude;
                    float sqrAttackRange = attackRange * attackRange;

                    if (sqrDistance <= sqrAttackRange)
                    {
                        // 在攻击范围内：停止移动并攻击
                        enemy.StopMoving();

                        // 检查是否可以进行攻击
                        if (!enemy.isAttacking && Time.time - enemy.lastAttackTime > 0.1f)
                        {
                            enemy.AttackCommand();
                        }
                    }
                    else
                    {
                        // 不在攻击范围内：向目标移动
                        Vector3 direction = (enemy.currentTarget.position - transform.position).normalized;
                        enemy.Move(direction);
                    }
                }
                else
                {
                    // 有玩家但没有目标：尝试重新索敌
                    enemy.AcquireTarget();
                }
            }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    // 检查是否有存活的玩家
    private bool CheckForAlivePlayers()
    {
        if (enemy.characterRoot == null) return false;

        foreach (Transform player in enemy.characterRoot)
        {
            // 检查标签是否为玩家
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