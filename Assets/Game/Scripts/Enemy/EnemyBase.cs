using TMPro;
using UnityEngine;

public class EnemyBase : CharacterBase
{
    [Header("Enemy Settings")]
    public Transform characterRoot; // 玩家角色根节点
    public string playerTag = "Player"; // 玩家标签
    public TextMeshProUGUI nameText;
    private string[] quality = { "", "普通", "精英", "稀有", "史诗", "传说" };
    private Color[] color = { Color.white,
        new Color(0.66f, 0.66f, 0.66f, 1f), //普通
        new Color(0.1f, 0.74f, 0f, 1f), //精英
        new Color(0.33f, 0.85f, 1f, 1f), //稀有
        new Color(0.68f, 0.3f, 1f, 1f), //史诗
        new Color(1f, 0.62f, 0.15f, 1f), //传说
    };

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
            var enemyData = StaticDataInterface.Enemy.GetEnemy(id);
            nameText.text = enemyData.Name + $"({quality[enemyData.Quality]})";
            nameText.color = color[enemyData.Quality];
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

    // 敌人专属攻击特效
    protected virtual void PlayEnemyAttackEffect(Vector3 position)
    {
        // 实际项目中这里会实例化敌人专属攻击特效
        //Debug.Log($"Enemy attack effect at {position}");
    }
}