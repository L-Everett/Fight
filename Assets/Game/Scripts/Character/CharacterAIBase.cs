using System.Collections;
using UnityEngine;

public class CharacterAIBase : MonoBehaviour
{
    public CharacterBase character;
    public float decisionInterval = 0.3f;
    private BattleManager battleManager;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        if (character == null) character = GetComponent<CharacterBase>();
        StartCoroutine(AIDecisionRoutine());
    }

    private IEnumerator AIDecisionRoutine()
    {
        while (true)
        {
            if (!character.isDead && battleManager.currentStage == BattleStage.Battle)
            {
                // 检查是否有存活的敌人
                bool hasAliveEnemies = CheckForAliveEnemies();

                if (!hasAliveEnemies)
                {
                    // 没有敌人时待机不动
                    character.StopMoving();
                }
                else if (character.currentTarget != null)
                {
                    // 计算到目标的距离
                    float attackRange = character.attributes.GetFinalAttr(
                        AttributeType.AttackRange, character.attributeModifiers);

                    float sqrDistance = (transform.position - character.currentTarget.position).sqrMagnitude;
                    float sqrAttackRange = attackRange * attackRange;

                    if (sqrDistance <= sqrAttackRange)
                    {
                        // 在攻击范围内：停止移动并攻击
                        character.StopMoving();
                        character.AttackCommand();
                    }
                    else
                    {
                        // 不在攻击范围内：向目标移动
                        Vector3 direction = (character.currentTarget.position - transform.position).normalized;
                        //Vector3 direction = character.currentTarget.position;
                        direction.y = 0;
                        character.Move(direction);
                    }
                }
                else
                {
                    // 有敌人但没有目标：尝试重新索敌
                    character.AcquireTarget();
                }
            }
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    // 检查是否有存活的敌人
    private bool CheckForAliveEnemies()
    {
        if (character.enemyRoot == null) return false;

        foreach (Transform enemy in character.enemyRoot)
        {
            CharacterBase enemyChar = enemy.GetComponent<CharacterBase>();
            if (enemyChar != null && !enemyChar.isDead)
            {
                return true;
            }
        }

        return false;
    }
}