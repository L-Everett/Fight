using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public FireOutlineController mFire;
    private CardLayoutController layoutController;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("卡牌属性")]
    public Image cardBgImg;
    public Image cardIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI qualityText;
    public TextMeshProUGUI contentText;
    private readonly string[] quality = { "", "普通", "精英", "稀有", "史诗", "传说" };

    // 卡牌图像子节点
    private Transform cardImageChild;
    private Transform scaleRoot;
    private Image cardImageComponent;
    private Vector3 cardImageOriginalScale;

    private Vector3 originalPosition;
    private Transform originalParent;

    private const float hoverScaleFactor = 1.5f;
    private const float dissolveScaleFactor = 3.2f;

    private bool isHovering = false;
    private bool isDragging = false;
    private string cardId;

    private float playAreaThreshold = 0.5f;

    // 溶解效果相关变量
    private GameObject dissolveEffectObject;
    private Material dissolveMaterial;
    private Texture2D dissolveTexture;
    private Coroutine dissolveCoroutine;
    private bool isPlayingCard = false;
    private Vector3 playPosition;

    private BattleManager battleManager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        scaleRoot = transform.Find("ScaleRoot");
        cardImageChild = scaleRoot.Find("CardImage");

        if (cardImageChild != null)
        {
            cardImageComponent = cardImageChild.GetComponent<Image>();
            cardImageOriginalScale = cardImageChild.localScale;
        }
        else
        {
            Debug.LogError("CardImage child not found!");
        }

        originalParent = transform.parent;

        layoutController = GetComponentInParent<CardLayoutController>();

        InitializeDissolveEffect();
    }

    private void Start()
    {
        battleManager = BattleManager.Instance;
    }

    private void InitializeDissolveEffect()
    {
        if (cardImageChild == null) return;

        dissolveEffectObject = new GameObject("DissolveEffect");
        dissolveEffectObject.transform.SetParent(transform);
        dissolveEffectObject.transform.position = cardImageChild.position;
        dissolveEffectObject.transform.rotation = cardImageChild.rotation;
        dissolveEffectObject.transform.localScale = cardImageChild.lossyScale * dissolveScaleFactor;

        CanvasRenderer renderer = dissolveEffectObject.AddComponent<CanvasRenderer>();
        Image dissolveImage = dissolveEffectObject.AddComponent<Image>();

        if (cardImageComponent != null)
        {
            dissolveImage.sprite = cardImageComponent.sprite;
            dissolveImage.preserveAspect = true;
        }
        dissolveImage.raycastTarget = false;

        dissolveMaterial = new Material(Shader.Find("Custom/DissolveShader"));
        dissolveImage.material = dissolveMaterial;
        dissolveMaterial.SetColor("_EdgeColor", new Color(1f, 0.5f, 0f, 1f));
        dissolveEffectObject.SetActive(false);
    }

    public void SetSharedNoiseTexture(Texture2D texture)
    {
        if (dissolveMaterial != null && texture != null)
        {
            dissolveMaterial.SetTexture("_DissolveTex", texture);
            dissolveTexture = texture;
        }
    }

    public void SetLayoutController(CardLayoutController controller)
    {
        layoutController = controller;
    }

    public void Init(Static_Card_t card)
    {
        //刷新卡牌属性
        cardId = card.Id;
        cardBgImg.sprite = Resources.Load<Sprite>($"Card/Card{card.Quality}");
        cardIcon.sprite = Resources.Load<Sprite>($"Card/{card.Icon}");
        nameText.text = card.Name;
        qualityText.text = "[" + quality[card.Quality] + "]";
        contentText.text = card.Des;

        dissolveEffectObject.GetComponent<Image>().sprite = cardBgImg.sprite;

        if (mFire != null)
        {
            mFire.enabled = true;
            mFire.mCardType = (CardType)card.Type;
            mFire.FreshColor();
        }
        ResetCardState();
    }

    public void ResetCardState()
    {
        isHovering = false;
        isDragging = false;
        isPlayingCard = false;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }

        if (cardImageChild != null)
        {
            cardImageChild.localScale = cardImageOriginalScale;
            cardImageChild.gameObject.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (dissolveEffectObject != null)
        {
            dissolveEffectObject.SetActive(false);
        }

        if (dissolveMaterial != null)
        {
            dissolveMaterial.SetFloat("_DissolveAmount", 0f);
        }
    }

    public void Destroy()
    {
        if (mFire != null)
        {
            mFire.enabled = false;
        }
    }

    #region 鼠标悬停事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (layoutController != null && layoutController.IsAnyCardDragging) return;
        if (isPlayingCard) return;

        if (isHovering || isDragging) return;
        isHovering = true;
        AudioManager.Instance.PlayHover();

        if (layoutController != null)
        {
            layoutController.HoverCard(rectTransform, true);
        }

        if (scaleRoot != null)
        {
            scaleRoot.localScale = Vector3.one * hoverScaleFactor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (layoutController != null && layoutController.IsAnyCardDragging) return;
        if (isPlayingCard) return;

        if (!isHovering || isDragging) return;
        isHovering = false;

        if (layoutController != null)
        {
            layoutController.HoverCard(rectTransform, false);
        }

        if (scaleRoot != null)
        {
            scaleRoot.localScale = Vector3.one;
        }
    }
    #endregion

    #region 拖拽事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (battleManager.currentStage != BattleStage.DoCard) return;
        if (isDragging || isPlayingCard) return;
        isDragging = true;

        if (layoutController != null)
        {
            layoutController.SetDraggingState(true);
        }

        if (isHovering)
        {
            isHovering = false;
            if (layoutController != null)
            {
                layoutController.HoverCard(rectTransform, false);
            }
        }

        originalPosition = rectTransform.anchoredPosition;
        originalPosition.y = 0;
        originalParent = transform.parent;

        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;

        if (cardImageChild != null)
        {
            cardImageChild.localScale = cardImageOriginalScale * 1.1f;
        }

        UpdateCardPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (battleManager.currentStage != BattleStage.DoCard) return;
        if (!isDragging || isPlayingCard) return;
        UpdateCardPosition(eventData.position);

        if (IsInPlayArea(eventData.position))
        {
            canvasGroup.alpha = 0.8f;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }

    private void UpdateCardPosition(Vector2 screenPosition)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isPlayingCard) return;
        isDragging = false;

        if (layoutController != null)
        {
            layoutController.SetDraggingState(false);
        }

        if (IsInPlayArea(eventData.position))
        {
            playPosition = rectTransform.anchoredPosition;
            PlayCard();
        }
        else
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
        if (scaleRoot != null)
        {
            scaleRoot.localScale = Vector3.one; // 立即重置根缩放
        }
    }
    #endregion

    #region 辅助方法
    private bool IsInPlayArea(Vector2 screenPosition)
    {
        float normalizedY = screenPosition.y / Screen.height;
        return normalizedY > playAreaThreshold;
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);

        Vector3 startPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float timer = 0f;

        Vector3 targetPosition = originalPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            rectTransform.anchoredPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress * progress
            );

            if (cardImageChild != null)
            {
                cardImageChild.localScale = Vector3.Lerp(
                    cardImageChild.localScale,
                    cardImageOriginalScale,
                    progress
                );
            }

            yield return null;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        rectTransform.anchoredPosition = targetPosition;
        if (cardImageChild != null)
        {
            cardImageChild.localScale = cardImageOriginalScale;
        }
        if (scaleRoot != null)
        {
            scaleRoot.localScale = Vector3.one; // 确保动画结束后完全重置
        }
    }

    private void PlayCard()
    {
        if (string.IsNullOrEmpty(cardId) || isPlayingCard) return;
        isPlayingCard = true;

        if (!Play(cardId))
        {
            isPlayingCard = false;
            StartCoroutine(ReturnToOriginalPosition());
            return;
        }
        StopAllCoroutines();
        canvasGroup.blocksRaycasts = false;

        if (cardImageChild != null)
        {
            cardImageChild.gameObject.SetActive(false);
        }

        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
        }
        dissolveCoroutine = StartCoroutine(PlayDissolveAnimation());
        AudioManager.Instance.PlayPlayCard();
    }

    private IEnumerator PlayDissolveAnimation()
    {
        if (dissolveEffectObject != null)
        {
            dissolveEffectObject.SetActive(true);
            if (cardImageChild != null)
            {
                dissolveEffectObject.transform.localScale = cardImageChild.lossyScale * dissolveScaleFactor;
            }
        }

        rectTransform.anchoredPosition = playPosition;
        float duration = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (dissolveMaterial != null)
            {
                dissolveMaterial.SetFloat("_DissolveAmount", progress);
            }
            yield return null;
        }

        ResetCardState();

        if (layoutController != null)
        {
            layoutController.RemoveCardImmediately(rectTransform);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private bool Play(string id)
    {
        return BattleManager.Instance.PlayCard(id);
    }

    void OnDestroy()
    {
        if (dissolveTexture != null)
        {
            Destroy(dissolveTexture);
        }
        if (dissolveMaterial != null)
        {
            Destroy(dissolveMaterial);
        }
        if (dissolveEffectObject != null)
        {
            Destroy(dissolveEffectObject);
        }
    }

    public bool IsPlayingCard => isPlayingCard;
    public Vector3 CardImageOriginalScale => cardImageOriginalScale;
}