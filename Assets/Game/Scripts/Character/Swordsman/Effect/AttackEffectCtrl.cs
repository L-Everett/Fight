using UnityEngine;
using System.Collections.Generic;

public class AttackEffectCtrl : MonoBehaviour
{
    [Header("特效设置")]
    public GameObject slashEffectPrefab; // 斩击特效Prefab
    public Transform effectRoot;         // 特效挂载根节点
    public Transform player;           // 玩家位置（特效生成点）

    [Header("对象池设置")]
    public int initialPoolSize = 5;      // 初始对象池大小
    public bool expandPool = true;        // 是否动态扩展对象池

    private Queue<GameObject> effectPool = new Queue<GameObject>();
    private List<GameObject> activeEffects = new List<GameObject>();

    void Start()
    {
        effectRoot = GameObject.Find("EffectRoot").transform;
        InitializeEffectPool();
    }

    // 初始化特效对象池
    private void InitializeEffectPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledEffect();
        }
    }

    // 创建新的特效实例并加入对象池
    private GameObject CreatePooledEffect()
    {
        GameObject effect = Instantiate(slashEffectPrefab, effectRoot);
        effect.SetActive(false);
        effectPool.Enqueue(effect);
        return effect;
    }

    // 从对象池获取特效实例
    private GameObject GetPooledEffect()
    {
        if (effectPool.Count == 0)
        {
            if (expandPool) return CreatePooledEffect();
            else return null;
        }

        return effectPool.Dequeue();
    }

    // 动画事件调用的特效播放方法
    public void PlaySlashEffect()
    {
        GameObject effect = GetPooledEffect();
        if (effect == null) return;

        // 设置特效位置和旋转（跟随剑尖）
        effect.transform.position = player.position;

        effect.SetActive(true);
        activeEffects.Add(effect);

        // 获取粒子系统组件并播放
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        // 启动回收协程
        StartCoroutine(ReturnEffectToPool(effect, ps.main.duration));
    }

    // 特效播放完成后回收到对象池
    private System.Collections.IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (effect != null)
        {
            effect.SetActive(false);
            activeEffects.Remove(effect);
            effectPool.Enqueue(effect);
        }
    }

    // 清理所有特效（场景切换时调用）
    public void CleanupAllEffects()
    {
        StopAllCoroutines();

        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
                effectPool.Enqueue(effect);
            }
        }
        activeEffects.Clear();
    }
}