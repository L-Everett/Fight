using System;
using System.Collections;
using UnityEngine;

public class EffectShockCtrl : MonoBehaviour
{
    private Vector3 InitPosition;
    private Vector3 targetPosition;

    private Action callback;
    private BattleManager battleManager;

    public float moveSpeed = 10f;

    void Awake()
    {
        InitPosition = transform.position;
    }

    public void SetCallback(Action callback)
    {
        this.callback = callback;
    }

    public void Init(Vector3 target)
    {
        if(battleManager == null) battleManager = BattleManager.Instance;
        // 设置位置和目标
        transform.position = InitPosition;
        targetPosition = target;
        gameObject.SetActive(true);

        StartCoroutine(MoveTarget());
    }

    IEnumerator MoveTarget()
    {
        // 计算移动方向
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.x = 0;
        direction.y = 0;

        while (true)
        {
            float moveDistance = moveSpeed * Time.deltaTime * battleManager.currentTimeSpeed;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (moveDistance > distanceToTarget)
            {
                moveDistance = distanceToTarget;
            }

            // 执行移动
            transform.position += direction * moveDistance;

            // 检查是否到达目标
            if (distanceToTarget <= 0.3f)
            {
                break;
            }
            yield return null;
        }
        callback?.Invoke();
        float time = 0.1f;
        if (BattleManager.Instance.currentTimeSpeed == 2) time = 0.05f;
        yield return new WaitForSeconds(time);
        DeactivateEffect();
    }

    private void DeactivateEffect()
    {
        gameObject.SetActive(false);
    }
}
