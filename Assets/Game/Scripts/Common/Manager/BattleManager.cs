using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleStage
{
    DoCard,   // 打牌阶段
    Battle,     // 战斗阶段
    Cleanup,     // 清理阶段
    End       //战斗结束
}
//todo: 1.技能    2.buff 

public class BattleManager : MonoBehaviour
{
    // 单例实例
    public static BattleManager Instance { get; private set; }

    [HideInInspector] public BattleStage currentStage;
    [HideInInspector] public int currentRound = 0;

    public TextMeshProUGUI roundText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI stageText;
    //敌人信息
    public TextMeshProUGUI enemyText;
    private string[] quality = { "", "普通", "精英", "稀有", "史诗", "传说" };
    private Color[] color = { Color.white,
        new Color(0.66f, 0.66f, 0.66f, 1f), //普通
        new Color(0.1f, 0.74f, 0f, 1f), //精英
        new Color(0.33f, 0.85f, 1f, 1f), //稀有
        new Color(0.68f, 0.3f, 1f, 1f), //史诗
        new Color(1f, 0.62f, 0.15f, 1f), //传说
    };
    public Transform enemyRoot;

    public Transform characterRoot;
    public GameObject nextStageBtn;
    public GameObject setting;
    public GameObject exit;
    public Image doubleSpeed;
    public GameObject lose;
    public GameObject win;

    [HideInInspector] public CharacterBase character;
    private Static_Round_t currentRoundData;

    [HideInInspector] public int characterCount = 0;
    [HideInInspector] public int enemyCount = 0;
    [HideInInspector] public float currentTimeSpeed = 1f;

    public GameObject skillPrefab;
    //临时增益UI
    public Transform leftRoot;
    private List<string> tempAdd = new List<string>();
    //持久增益UI
    public Transform rightRoot;
    private int uid = 0;
    private List<string> cardsHasGet = new List<string>();

    //金币数
    private int coinCount = 0;
    //金币加成
    [HideInInspector] public float coinAdd = 0;
    //难度增幅
    [HideInInspector] public float[] diffAdd = { 1, 2, 5, 10 };
    //最大回合数
    [HideInInspector] public int maxRoundCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Transform characterTrans = GameObject.Instantiate(
            Resources.Load<GameObject>($"Character/{RunningManager.Instance.mCurrentCharacter}"),
            characterRoot).transform;
        character = characterTrans.GetComponent<CharacterBase>();
        character.Init();
        //初始资金
        coinCount = 400;
        coinText.text = coinCount.ToString();
        gemText.text = DataModel.Instance.mGemCount.ToString();

        maxRoundCount = RunningManager.Instance.mMaxRound;

        StartNewRound();
    }

    private void Update()
    {
        CheckBattleEnd();
        KeyGetESC();
    }

    // 开始新回合
    public void StartNewRound()
    {
        currentRound++;
        if (currentRound > maxRoundCount)
        {
            StartCoroutine(DelayWin());
            return;
        }
        roundText.text = $"第<color=#FF0000>{currentRound}</color>轮";

        //通知新回合开始
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_ROUND_START);
        //角色相关刷新
        character.transform.localPosition = Vector3.zero;
        characterCount = 1;
        //通知回合增益开始刷新
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_ROUND_FRESH);

        //刷怪
        currentRoundData = StaticDataInterface.Round.GetRound(currentRound);
        List<string> enemylist = currentRoundData.EnemyList;
        enemyCount = enemylist.Count;
        foreach (string enemy in enemylist)
        {
            var e = GameObject.Instantiate(Resources.Load<GameObject>($"Enemy/{enemy}"),
                enemyRoot).GetComponent<CharacterBase>();
            e.SetID(enemy);
            e.Init();
            float add = diffAdd[RunningManager.Instance.mCurrentDiff];
            AttributeModifier hp = new AttributeModifier(AttributeType.Hp, currentRoundData.HpAdd * add, true);
            AttributeModifier atk = new AttributeModifier(AttributeType.Attack, currentRoundData.ATKAdd * add, true);
            e.AddAttributeModifier(hp);
            e.AddAttributeModifier(atk);
            var enemyData = StaticDataInterface.Enemy.GetEnemy(enemy);
            enemyText.text = enemyData.Name + $"({quality[enemyData.Quality]})";
            enemyText.color = color[enemyData.Quality];
        }

        currentStage = BattleStage.DoCard;
        stageText.text = "打牌阶段";
        nextStageBtn.SetActive(true);
    }

    IEnumerator DelayWin()
    {
        yield return new WaitForSeconds(0.8f);
        Win();
    }

    //抽卡
    public string AddCard(CardType cardType)
    {
        string id = string.Empty;
        var cards = StaticDataInterface.Card.GetCardsByType(cardType);

        //过滤
        List<string> finalCard = new List<string>();
        List<int> finalWeight = new List<int>();
        for(int i = 0; i < cards.Item1.Count; i++)
        {
            if (!cardsHasGet.Contains(cards.Item1[i]))
            {
                finalCard.Add(cards.Item1[i]);
                finalWeight.Add(cards.Item2[i]);
            }
        }

        int index = Utils.GetRandomIndex(finalWeight);
        if (index != -1)
        {
            id = finalCard[index];
            if (cardType == CardType.LeftSide)
            {
                if (coinCount < 50)
                {
                    MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "金币不足!");
                    return string.Empty;
                }
                coinCount -= 50;
            }
            else
            {
                if (coinCount < 300)
                {
                    MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "金币不足!");
                    return string.Empty;
                }
                coinCount -= 300;
            }
        }
        coinText.text = coinCount.ToString();
        cardsHasGet.Add(id);

        return id;
    }

    //打出卡牌
    public bool PlayCard(string id)
    {
        var card = StaticDataInterface.Card.GetCard(id);
        if (card == null) return false;
        if (card.Type == 1) //临时卡
        {
            if (tempAdd.Contains(id))
            {
                MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "该卡牌生效中！");
                return false;
            }
            tempAdd.Add(id);

            //左侧UI显示
            var skill = GameObject.Instantiate(skillPrefab, leftRoot).GetComponent<SkillCtrl>();
            skill.Init(id, uid++);
        }
        else  //持久卡
        {
            //右侧UI显示
            var skill = GameObject.Instantiate(skillPrefab, rightRoot).GetComponent<SkillCtrl>();
            skill.Init(id, uid++);
        }
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SKILL_ADD, (id, uid));
        return true;
    }

    // 出牌完毕
    public void BattleStart()
    {
        currentStage = BattleStage.Battle;
        stageText.text = "战斗阶段";
        nextStageBtn.SetActive(false);
    }

    // 检查战斗情况
    private void CheckBattleEnd()
    {
        if (currentStage == BattleStage.Battle)
        {
            if (characterCount == 0)
            {
                Lose();
            }
            else if (enemyCount == 0)
            {
                HandleCleanupPhase();
            }
        }
    }

    // 清理阶段逻辑
    private void HandleCleanupPhase()
    {
        currentStage = BattleStage.Cleanup;
        stageText.text = "结算阶段";

        if(currentRound + 1 > maxRoundCount)
        {
            //游戏胜利
        }
        else
        {
            float roundCoin = StaticDataInterface.Round.GetRound(currentRound).Coin;
            AddCoin(roundCoin);
            float add = diffAdd[RunningManager.Instance.mCurrentDiff];
            AddGem(add);
            // 重置状态，准备下一回合
            nextStageBtn.SetActive(true);
        }
        StartNewRound();
    }

    public void AddCoin(float count)
    {
        count += count * coinAdd;
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, $"金币 +{count}！");
        coinCount += (int)count;
        coinText.text = coinCount.ToString();
    }

    public void AddGem(float count)
    {
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, $"钻石 +{count}！");
        DataModel.Instance.mGemCount += (int)count;
        gemText.text = DataModel.Instance.mGemCount.ToString();
    }

    void Win()
    {
        Pause();
        currentStage = BattleStage.End;
        AudioManager.Instance.PlayWin();
        win.SetActive(true);
    }

    void Lose()
    {
        Pause();
        currentStage = BattleStage.End;
        AudioManager.Instance.PlayLose();
        lose.SetActive(true);
    }

    public void ReturnStart()
    {
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SCENE_CHANGE, "StartScene");
    }

    #region 按钮
    private bool isPause = false;
    private float lastTimeSpeed = 1f;
    public void Pause()
    {
        if (isPause) return;
        currentTimeSpeed = 0;
        isPause = true;
    }

    public void OpenSetting()
    {
        Pause();
        setting.SetActive(true);
    }

    public void Resume()
    {
        if (!isPause) return;
        currentTimeSpeed = lastTimeSpeed;
        isPause = false;
    }

    public void DoubleSpeed()
    {
        if (currentTimeSpeed == 1)
        {
            currentTimeSpeed = 2f;
            lastTimeSpeed = 2f;
            doubleSpeed.sprite = Resources.Load<Sprite>("MainUI/Two");
        }
        else
        {
            currentTimeSpeed = 1f;
            lastTimeSpeed = 1f;
            doubleSpeed.sprite = Resources.Load<Sprite>("MainUI/One");
        }
    }

    [HideInInspector] public bool isOnESC = false;
    public void ESC()
    {
        isOnESC = true;
        Pause();
        exit.SetActive(true);
    }

    void KeyGetESC()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isOnESC && !isPause)
        {
            ESC();
        }
    }
    #endregion

    #region GM
    public void GM_Coin()
    {
        coinCount += 1000;
        coinText.text = coinCount.ToString();
    }
    public void GM_Gem()
    {
        DataModel.Instance.mGemCount += 1000;
        gemText.text = DataModel.Instance.mGemCount.ToString();
    }
    #endregion
}