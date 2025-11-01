using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IMsgHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI desText;
    [Header("UI References")]
    public GameObject tooltipPanelPrefab; // 弹窗预制体
    public float hoverDelay = 2f; // 悬停延迟时间（秒）

    private string skillDescription = ""; 

    private int lifeTime = 100;
    private string skillID = string.Empty;
    private int skillUID = -1;
    bool isShow = false;

    void Start()
    {
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_ROUND_START, this);
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
        if (isShow)
        {
            ShowInfoManager.Instance.HideInfo();
        }
    }

    public void Init(string id, int uid)
    {
        var card = StaticDataInterface.Card.GetCard(id);
        nameText.text = card.Name;
        skillDescription = card.Des;
        desText.text = card.Des;
        lifeTime = card.LifeTime;
        skillID = card.SkillId;
        skillUID = uid;
    }

    // 鼠标进入技能UI区域
    public void OnPointerEnter(PointerEventData eventData)
    {
        isShow = true;
        ShowInfoManager.Instance.ShowInfo(skillDescription);
    }

    // 鼠标离开技能UI区域
    public void OnPointerExit(PointerEventData eventData)
    {
        isShow = false;
        ShowInfoManager.Instance.HideInfo();
    }

    public void Handle(string msg, object obj)
    {
        if(msg == Constant.MSG_NOTIFY_ROUND_START)
        {
            lifeTime--;
            if (lifeTime <= 0)
            {
                MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SKILL_END, (skillID, skillUID));
                Destroy(gameObject);
            }
        }
    }
}
