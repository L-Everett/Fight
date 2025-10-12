using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class SkillCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IMsgHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI desText;
    [Header("UI References")]
    public GameObject tooltipPanelPrefab; // 弹窗预制体
    public float hoverDelay = 2f; // 悬停延迟时间（秒）

    private string skillDescription = ""; 

    private GameObject currentTooltip; // 当前显示的弹窗实例
    private Coroutine hoverCoroutine; // 悬停协程
    private bool isHoveringSkill; // 是否悬停在技能上
    private bool isHoveringTooltip; // 是否悬停在弹窗上
    private Canvas parentCanvas; // 父级Canvas

    private int lifeTime = 100;
    private string skillID = string.Empty;
    private int skillUID = -1;

    void Start()
    {
        // 获取父级Canvas
        parentCanvas = GetComponentInParent<Canvas>();
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_ROUND_START, this);
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
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
        isHoveringSkill = true;
        hoverCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    // 鼠标离开技能UI区域
    public void OnPointerExit(PointerEventData eventData)
    {
        isHoveringSkill = false;

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        // 检查是否需要隐藏弹窗
        CheckHideTooltip();
    }

    // 延迟显示弹窗
    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);

        if (isHoveringSkill)
        {
            ShowTooltip();
        }
    }

    // 显示技能详情弹窗
    private void ShowTooltip()
    {
        if (tooltipPanelPrefab == null || parentCanvas == null) return;

        // 如果还没有实例化弹窗，创建新实例
        if (currentTooltip == null)
        {
            currentTooltip = Instantiate(tooltipPanelPrefab, parentCanvas.transform);
            currentTooltip.name = "SkillTooltip";

            // 设置描述文本
            var descriptionText = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
            if (descriptionText != null)
            {
                descriptionText.text = skillDescription;
            }

            // 添加悬停检测组件
            var tooltipHover = currentTooltip.AddComponent<TooltipHoverDetector>();
            tooltipHover.Initialize(this);
        }

        // 定位弹窗位置
        PositionTooltip();

        // 显示弹窗
        currentTooltip.SetActive(true);
    }

    // 定位弹窗
    private void PositionTooltip()
    {
        if (currentTooltip == null) return;

        RectTransform skillRect = GetComponent<RectTransform>();
        RectTransform tooltipRect = currentTooltip.GetComponent<RectTransform>();

        // 计算位置（在技能UI右侧，与技能UI顶部对齐）
        Vector3 position = skillRect.position;
        position.x += skillRect.rect.width * 0.5f + tooltipRect.rect.width * 0.5f + 10f;

        tooltipRect.position = position;
    }

    // 通知悬停在弹窗上
    public void NotifyTooltipHover(bool isHovering)
    {
        isHoveringTooltip = isHovering;
        CheckHideTooltip();
    }

    // 检查是否需要隐藏弹窗
    private void CheckHideTooltip()
    {
        // 只有当不在技能上也不在弹窗上时才隐藏
        if (!isHoveringSkill && !isHoveringTooltip)
        {
            HideTooltip();
        }
    }

    // 隐藏技能详情弹窗
    private void HideTooltip()
    {
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }
    }

    // 当脚本被禁用时确保清理
    private void OnDisable()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        HideTooltip();
        isHoveringSkill = false;
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

// 弹窗悬停检测组件
public class TooltipHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SkillCtrl parentSkillCtrl;

    public void Initialize(SkillCtrl skillCtrl)
    {
        parentSkillCtrl = skillCtrl;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentSkillCtrl?.NotifyTooltipHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentSkillCtrl?.NotifyTooltipHover(false);
    }
}