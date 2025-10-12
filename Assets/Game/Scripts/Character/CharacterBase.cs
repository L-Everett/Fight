using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NPOI.HSSF.Util.HSSFColor;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("Core Components")]
    public CharacterAttributes attributes;
    public Animator animator;
    public Rigidbody rb;
    [HideInInspector] public Transform attackPoint;
    public HPCtrl hpCtrl;
    public Collider hitCollider;
    public NavMeshAgent navMeshAgent;

    [Header("Combat Settings")]
    public Transform enemyRoot; // 敌人根节点
    public float acquireInterval = 0.5f; // 索敌间隔时间

    [Header("Runtime State")]
    public bool isMoving;
    public bool isAttacking;
    public bool isDead;
    public Transform currentTarget;
    public float lastAttackTime;
    public float lastAcquireTime; // 上次索敌时间

    [HideInInspector] public Dictionary<int, AttributeModifier> attributeModifiers = new Dictionary<int, AttributeModifier>();
    private int attrUid = 0;
    protected float currentHp;
    protected string id = string.Empty;
    protected bool isEnemy = false;

    protected BattleManager battleManager;

    protected virtual void Awake()
    {
        // 确保必要的组件存在
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (hpCtrl == null) hpCtrl = GetComponentInChildren<HPCtrl>();
        if (hitCollider == null) hitCollider = GetComponent<Collider>();

        // 自动查找敌人根节点
        if (enemyRoot == null)
        {
            GameObject rootObj = GameObject.Find("EnemyRoot");
            if (rootObj != null) enemyRoot = rootObj.transform;
        }
    }

    protected virtual void Start()
    {
        Init();
    }

    private void Update()
    {
        animator.speed = battleManager.currentTimeSpeed;
        UpdateAnimationSpeed();
    }

    public virtual void SetID(string id)
    {
        this.id = id;
        isEnemy = true;
    }

    public virtual void Init()
    {
        battleManager = BattleManager.Instance;
        // 初始化属性
        attributes.Init(RunningManager.Instance.mCurrentCharacter);

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
        //UpdateAnimationSpeed();
    }

    // 更新动画速度（根据攻速）
    protected virtual void UpdateAnimationSpeed()
    {
        float attackSpeed = attributes.GetFinalAttr(AttributeType.AttackSpeed, attributeModifiers);
        animator.SetFloat("AttackSpeed", attackSpeed * battleManager.currentTimeSpeed);
    }

    // 添加属性修饰符
    public int AddAttributeModifier(AttributeModifier modifier)
    {
        int id = attrUid++;
        attributeModifiers.Add(id, modifier);
        attributes.MarkDirty();

        // 如果是攻速相关修饰符，更新动画速度
        if (modifier.attributeType == AttributeType.AttackSpeed)
        {
            //UpdateAnimationSpeed();
        }

        // 如果是HP相关修饰符，更新HP显示
        //if (modifier.attributeType == AttributeType.Hp)
        //{
        //    float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        //    hpCtrl.FreshHp(currentHp, maxHp);
        //}

        return id;
    }

    // 移除属性修饰符
    public void RemoveAttributeModifier(int id)
    {
        if (id < 0) return;
        attributeModifiers.TryGetValue(id, out AttributeModifier modifier);
        attributes.MarkDirty();

        // 如果是攻速相关修饰符，更新动画速度
        if (modifier.attributeType == AttributeType.AttackSpeed)
        {
            //UpdateAnimationSpeed();
        }

        // 如果是HP相关修饰符，更新HP显示
        if (modifier.attributeType == AttributeType.Hp)
        {
            float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
            hpCtrl.FreshHp(currentHp, maxHp);
        }
        attributeModifiers.Remove(id);
    }

    // 清除所有属性修饰符
    public void ClearAllAttributeModifiers()
    {
        attributeModifiers.Clear();
        attributes.MarkDirty();

        // 更新动画速度
        //UpdateAnimationSpeed();

        // 更新HP显示
        float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        hpCtrl.FreshHp(currentHp, maxHp);
    }

    // 移动系统
    public virtual void Move(Vector3 direction)
    {
        if (isDead || isAttacking) return;

        isMoving = direction.magnitude > 0.1f;
        float moveSpeed = attributes.GetFinalAttr(AttributeType.MoveSpeed, attributeModifiers);
        //rb.velocity = direction * moveSpeed;
        animator.SetFloat("Speed", direction.magnitude);

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(direction);
        navMeshAgent.speed = moveSpeed;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // 停止移动
    public virtual void StopMoving()
    {
        rb.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;
        animator.SetFloat("Speed", 0f);
        isMoving = false;
    }

    // 目标锁定
    public virtual void AcquireTarget()
    {
        // 如果当前目标有效且距离合适，保持当前目标
        if (currentTarget != null) return;

        // 重置当前目标
        currentTarget = null;
        float closestSqrDistance = float.MaxValue;

        // 遍历EnemyRoot下的所有敌人
        if (enemyRoot != null)
        {
            foreach (Transform enemy in enemyRoot)
            {
                // 检查标签是否为敌人
                if (!enemy.CompareTag("Enemy")) continue;

                CharacterBase enemyCharacter = enemy.GetComponent<CharacterBase>();
                if (enemyCharacter == null || enemyCharacter.isDead) continue;

                float sqrDistance = (transform.position - enemy.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    currentTarget = enemy;
                }
            }
        }
    }

    // 攻击接口（供AI调用）
    public virtual void AttackCommand()
    {
        if (!isAttacking && !isDead && currentTarget != null && Time.time - lastAttackTime > 0.1f)
        {
            StartAttack();
        }
    }

    // 开始攻击
    protected virtual void StartAttack()
    {
        StartCoroutine(AttackRoutine());
        lastAttackTime = Time.time;
    }

    // 攻击协程（受攻速影响）
    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        // 获取当前攻速
        float attackSpeed = attributes.GetFinalAttr(AttributeType.AttackSpeed, attributeModifiers);
        
        // 攻击前摇（受攻速影响）
        float preSwingTime = 0.3f / attackSpeed;
        yield return new WaitForSeconds(preSwingTime);

        animator.SetTrigger("Attack");
        //animator.SetFloat("AttackSpeed", attackSpeed * battleManager.currentTimeSpeed);
    }

    // 伤害计算
    public virtual float CalculateDamage(out bool isCrit, float rate = 1f)
    {
        float baseDmg = attributes.GetFinalAttr(AttributeType.Attack, attributeModifiers) * rate;
        float critRate = attributes.GetFinalAttr(AttributeType.CritRate, attributeModifiers);
        isCrit = Random.value <= critRate;

        if (isCrit)
        {
            float critDamage = attributes.GetFinalAttr(AttributeType.CritDamage, attributeModifiers);
            //Debug.Log(baseDmg * (1 + critDamage));
            return baseDmg * (1 + critDamage);
        }

        float damageAddRate = attributes.GetFinalAttr(AttributeType.DamageAddRate, attributeModifiers);
        baseDmg += baseDmg * damageAddRate;

        return baseDmg;
    }

    // 受伤处理
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        hpCtrl.FreshHp(currentHp, maxHp);

        //animator.SetTrigger("Hurt");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 死亡处理
    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        rb.velocity = Vector3.zero;

        if (hitCollider != null) hitCollider.enabled = false;
        rb.isKinematic = true;

        isAttacking = false;
        isMoving = false;
        currentTarget = null;

        StartCoroutine(DeathRoutine());
    }

    protected virtual IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(2f);
        if(isEnemy)
        {
            BattleManager.Instance.enemyCount--;
        }
        else
        {
            BattleManager.Instance.characterCount--;
        }

        if (isEnemy)
        {
            battleManager.AddCoin(StaticDataInterface.Enemy.GetEnemy(id).Coin);
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 治疗
    public virtual void Heal(float amount)
    {
        if (isDead) return;
        Vector3 popupPosition = transform.position + Vector3.up * 2f;
        DamagePopupManager.Instance.ShowHeal(popupPosition, amount);
        float maxHp = attributes.GetFinalAttr(AttributeType.Hp, attributeModifiers);
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        hpCtrl.FreshHp(currentHp, maxHp);
    }
}