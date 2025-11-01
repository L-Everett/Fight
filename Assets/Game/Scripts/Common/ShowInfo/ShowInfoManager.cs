using UnityEngine;

public class ShowInfoManager : MonoBehaviour
{
    // 单例实例
    private static ShowInfoManager _instance;
    public static ShowInfoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ShowInfoManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ShowInfoManager");
                    _instance = go.AddComponent<ShowInfoManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("预制体设置")]
    public GameObject currentInfoObject;
    public Canvas targetCanvas;

    [Header("显示设置")]
    public Vector2 offset = new Vector2(20, 20);
    public float edgePadding = 10f;
    public float showDelay = 0.3f;

    private InfoCtrl currentInfoCtrl;

    private float showTimer = 0f;
    private bool isShowing = false;
    private Vector3 initialHoverPosition;
    private string pendingContent;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (currentInfoObject != null)
            currentInfoCtrl = currentInfoObject.GetComponent<InfoCtrl>();
    }

    void Update()
    {
        // 延迟显示逻辑
        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0 && !string.IsNullOrEmpty(pendingContent))
            {
                ShowInfoImmediate(pendingContent, initialHoverPosition);
            }
        }
    }

    /// <summary>
    /// 显示信息提示
    /// </summary>
    /// <param name="content">显示内容</param>
    public void ShowInfo(string content)
    {
        // 记录初始悬停位置
        initialHoverPosition = Input.mousePosition;
        InternalShowInfo(content, initialHoverPosition);
    }

    /// <summary>
    /// 隐藏信息提示
    /// </summary>
    public void HideInfo()
    {
        InternalHideInfo();
    }

    private void InternalShowInfo(string content, Vector3 position)
    {
        if (isShowing)
        {
            InternalHideInfo();
        }

        pendingContent = content;
        showTimer = showDelay;
    }

    private void ShowInfoImmediate(string content, Vector3 position)
    {
        if (currentInfoObject == null || currentInfoCtrl == null)
        {
            Debug.LogError("CurrentInfoObject or InfoCtrl is not set!");
            return;
        }

        // 初始化内容
        currentInfoCtrl.Init(content);
        currentInfoObject.SetActive(true);

        // 基于初始位置计算最终显示位置
        CalculateAndSetPosition(position);

        isShowing = true;
        pendingContent = null;
    }

    private void InternalHideInfo()
    {
        showTimer = 0f;
        pendingContent = null;

        if (currentInfoObject != null)
        {
            currentInfoObject.SetActive(false);
        }

        isShowing = false;
    }

    private void CalculateAndSetPosition(Vector3 hoverPosition)
    {
        if (currentInfoObject == null || targetCanvas == null) return;

        RectTransform rectTransform = currentInfoObject.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 获取弹窗实际尺寸
        Vector2 tooltipSize = rectTransform.rect.size * targetCanvas.scaleFactor;

        // 计算屏幕中心点
        float screenCenterX = Screen.width / 2f;

        Vector2 finalPosition;

        // 判断鼠标在屏幕左侧还是右侧
        if (hoverPosition.x < screenCenterX)
        {
            // 鼠标在左侧，弹窗显示在右侧
            finalPosition = new Vector2(
                hoverPosition.x + offset.x + tooltipSize.x / 2,
                hoverPosition.y + offset.y + tooltipSize.y / 2
            );

            // 检查右侧边界
            if (finalPosition.x + tooltipSize.x > Screen.width - edgePadding)
            {
                finalPosition.x = Screen.width - tooltipSize.x - edgePadding;
            }
        }
        else
        {
            // 鼠标在右侧，弹窗显示在左侧
            finalPosition = new Vector2(
                hoverPosition.x - offset.x - tooltipSize.x / 2,
                hoverPosition.y + offset.y + tooltipSize.y /2
            );

            // 检查左侧边界
            if (finalPosition.x < edgePadding)
            {
                finalPosition.x = edgePadding;
            }
        }

        // 垂直方向边界检查
        if (finalPosition.y - tooltipSize.y < edgePadding) // 底部检查
        {
            finalPosition.y = tooltipSize.y + edgePadding;
        }
        else if (finalPosition.y > Screen.height - edgePadding) // 顶部检查
        {
            finalPosition.y = Screen.height - edgePadding;
        }

        // 转换为Canvas局部坐标并设置位置
        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas.transform as RectTransform,
            finalPosition,
            targetCanvas.worldCamera,
            out localPosition))
        {
            rectTransform.anchoredPosition = localPosition;
        }
    }
}