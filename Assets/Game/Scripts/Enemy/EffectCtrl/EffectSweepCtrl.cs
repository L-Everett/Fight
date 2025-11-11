using System;
using System.Collections;
using UnityEngine;

public class EffectSweepCtrl : MonoBehaviour
{
    private Vector3 InitPosition;

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
        if (battleManager == null) battleManager = BattleManager.Instance;
        // 设置位置和目标
        transform.position = InitPosition + target;
        gameObject.SetActive(true);

        StartCoroutine(MoveTarget());
    }

    IEnumerator MoveTarget()
    {
        float time = 0.1f;
        if (BattleManager.Instance.currentTimeSpeed == 2) time = 0.05f;
        yield return new WaitForSeconds(time);
        callback?.Invoke();
        time = 1.2f;
        if (BattleManager.Instance.currentTimeSpeed == 2) time = 0.6f;
        yield return new WaitForSeconds(time);
        DeactivateEffect();
    }

    private void DeactivateEffect()
    {
        gameObject.SetActive(false);
    }
}
