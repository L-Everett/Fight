using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLayoutController : MonoBehaviour
{
    [Header("布局设置")]
    public float maxAreaWidth = 800f;
    public float minCardSpacing = 50f;
    public float maxCardSpacing = 150f;
    public float transitionDuration = 0.3f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("位置偏移")]
    public Vector2 horizontalPosition = new Vector2(0f, 0f);

    [Header("动画效果")]
    public float hoverHeight = 20f;
    public float hoverDuration = 0.2f;

    [Header("卡牌设置")]
    public List<RectTransform> cards = new List<RectTransform>();

    [Header("卡牌预制体")]
    public GameObject cardPrefab;

    private Dictionary<RectTransform, Coroutine> activeAnimations = new Dictionary<RectTransform, Coroutine>();
    private Queue<GameObject> cardPool = new Queue<GameObject>();
    private RectTransform currentHoveredCard;
    private Dictionary<RectTransform, Vector3> originalPositions = new Dictionary<RectTransform, Vector3>();

    public bool IsAnyCardDragging { get; private set; }
    private bool isAnyAnimationPlaying = false;

    private Queue<IEnumerator> animationQueue = new Queue<IEnumerator>();
    private bool isProcessingAnimation = false;

    private Queue<CardAddRequest> cardAddQueue = new Queue<CardAddRequest>();
    private bool isProcessingAddAnimation = false;

    private struct CardAddRequest
    {
        public string id;
        public CardAddRequest(string cardId)
        {
            id = cardId;
        }
    }

    private Texture2D sharedNoiseTexture;

    void Start()
    {
        sharedNoiseTexture = GenerateSharedNoiseTexture(256, 256);
        ArrangeCards();
    }

    private Texture2D GenerateSharedNoiseTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Random.value;
                tex.SetPixel(x, y, new Color(noise, noise, noise));
            }
        }
        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        if (sharedNoiseTexture != null)
        {
            Destroy(sharedNoiseTexture);
        }
    }

    public void SetDraggingState(bool isDragging)
    {
        if (isAnyAnimationPlaying && isDragging) return;
        IsAnyCardDragging = isDragging;
    }

    public void AddCard(string id)
    {
        cardAddQueue.Enqueue(new CardAddRequest(id));
        if (!isProcessingAddAnimation)
        {
            StartCoroutine(ProcessCardAddQueue());
        }
    }

    private IEnumerator ProcessCardAddQueue()
    {
        isProcessingAddAnimation = true;
        isAnyAnimationPlaying = true;

        while (cardAddQueue.Count > 0)
        {
            CardAddRequest request = cardAddQueue.Dequeue();
            yield return StartCoroutine(ProcessSingleCardAdd(request.id));
        }

        isProcessingAddAnimation = false;
        isAnyAnimationPlaying = false;
    }

    private IEnumerator ProcessSingleCardAdd(string id)
    {
        GameObject cardObj = GetCardInPool();
        if (cardObj == null) yield break;

        var card = StaticDataInterface.Card.GetCard(id);
        CardType cardType = (CardType)card.Type;

        CardCtrl cardCtrl = cardObj.GetComponent<CardCtrl>();
        cardCtrl.Init(card);
        cardCtrl.SetLayoutController(this);

        if (sharedNoiseTexture != null)
        {
            cardCtrl.SetSharedNoiseTexture(sharedNoiseTexture);
        }

        RectTransform newCard = cardObj.GetComponent<RectTransform>();

        if (cardType == CardType.LeftSide)
        {
            cards.Insert(0, newCard);
        }
        else
        {
            cards.Add(newCard);
        }

        UpdateCardDepths();
        yield return StartCoroutine(AddCardAnimationCoroutine(newCard, cardType));
    }

    GameObject GetCardInPool()
    {
        while (cardPool.Count > 0)
        {
            GameObject card = cardPool.Dequeue();
            if (card != null)
            {
                card.SetActive(true);
                CardCtrl cardCtrl = card.GetComponent<CardCtrl>();
                if (cardCtrl != null)
                {
                    cardCtrl.ResetCardState();
                }
                RectTransform rect = card.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                return card;
            }
        }

        if (cards.Count < 10)
        {
            return GameObject.Instantiate(cardPrefab, transform);
        }

        return null;
    }

    public void RemoveCardImmediately(RectTransform cardToRemove)
    {
        if (cards.Contains(cardToRemove))
        {
            if (originalPositions.ContainsKey(cardToRemove))
            {
                originalPositions.Remove(cardToRemove);
            }

            if (currentHoveredCard == cardToRemove)
            {
                currentHoveredCard = null;
            }

            cards.Remove(cardToRemove);
            UpdateCardDepths();
            EnqueueAnimation(ArrangeCardsAnimationCoroutine());
            ReturnCardToPool(cardToRemove.gameObject);
        }
    }

    public void ReturnCardToPool(GameObject cardObj)
    {
        if (cardObj == null) return;

        var cardCtrl = cardObj.GetComponent<CardCtrl>();
        if (cardCtrl != null)
        {
            cardCtrl.ResetCardState();
        }

        cardObj.transform.SetParent(transform);
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        cardObj.SetActive(false);
        cardPool.Enqueue(cardObj);
    }

    public void ArrangeCards()
    {
        if (cards.Count == 0) return;

        float dynamicSpacing = CalculateDynamicSpacing();
        float totalWidth = (cards.Count - 1) * dynamicSpacing;
        Vector3 startPos = new Vector3(
            horizontalPosition.x - totalWidth / 2f,
            horizontalPosition.y,
            0
        );

        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 targetPosition = startPos + new Vector3(i * dynamicSpacing, 0, 0);
            cards[i].anchoredPosition = targetPosition;

            if (originalPositions.ContainsKey(cards[i]))
            {
                originalPositions[cards[i]] = targetPosition;
            }
            else
            {
                originalPositions.Add(cards[i], targetPosition);
            }
        }

        UpdateCardDepths();
    }

    float CalculateDynamicSpacing()
    {
        if (cards.Count <= 1) return maxCardSpacing;
        float idealSpacing = maxAreaWidth / (cards.Count - 1);
        return Mathf.Clamp(idealSpacing, minCardSpacing, maxCardSpacing);
    }

    private void EnqueueAnimation(IEnumerator animationCoroutine)
    {
        animationQueue.Enqueue(animationCoroutine);
        if (!isProcessingAnimation)
        {
            StartCoroutine(ProcessAnimationQueue());
        }
    }

    private IEnumerator ProcessAnimationQueue()
    {
        isProcessingAnimation = true;
        isAnyAnimationPlaying = true;

        while (animationQueue.Count > 0)
        {
            IEnumerator nextAnimation = animationQueue.Dequeue();
            yield return StartCoroutine(nextAnimation);
        }

        isProcessingAnimation = false;
        isAnyAnimationPlaying = false;
    }

    private IEnumerator AddCardAnimationCoroutine(RectTransform newCard, CardType cardType)
    {
        while (isProcessingAnimation)
        {
            yield return null;
        }
        yield return StartCoroutine(AddCardAnimation(newCard, cardType));
    }

    private IEnumerator ArrangeCardsAnimationCoroutine()
    {
        yield return StartCoroutine(ArrangeCardsAnimation());
    }

    private IEnumerator ArrangeCardsAnimation()
    {
        List<RectTransform> currentCards = new List<RectTransform>(cards);
        if (currentCards.Count == 0) yield break;

        float dynamicSpacing = CalculateDynamicSpacing();
        Vector3[] startPositions = new Vector3[currentCards.Count];
        for (int i = 0; i < currentCards.Count; i++)
        {
            startPositions[i] = currentCards[i].anchoredPosition;
        }

        Vector3[] targetPositions = new Vector3[currentCards.Count];
        float totalWidth = (currentCards.Count - 1) * dynamicSpacing;
        Vector3 startPos = new Vector3(
            horizontalPosition.x - totalWidth / 2f,
            horizontalPosition.y,
            0
        );

        for (int i = 0; i < currentCards.Count; i++)
        {
            targetPositions[i] = startPos + new Vector3(i * dynamicSpacing, 0, 0);
        }

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);
            float curveProgress = moveCurve.Evaluate(progress);

            for (int i = 0; i < currentCards.Count; i++)
            {
                if (!cards.Contains(currentCards[i])) continue;

                float distance = Vector3.Distance(startPositions[i], targetPositions[i]);
                float adjustedProgress = Mathf.Clamp01(curveProgress * (distance / dynamicSpacing));

                currentCards[i].anchoredPosition = Vector3.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    adjustedProgress
                );

                float yOffset = Mathf.Sin(adjustedProgress * Mathf.PI) * hoverHeight;
                currentCards[i].anchoredPosition += new Vector2(0, yOffset);
            }

            yield return null;
        }

        for (int i = 0; i < currentCards.Count; i++)
        {
            if (!cards.Contains(currentCards[i])) continue;

            currentCards[i].anchoredPosition = targetPositions[i];

            if (originalPositions.ContainsKey(currentCards[i]))
            {
                originalPositions[currentCards[i]] = targetPositions[i];
            }
            else
            {
                originalPositions.Add(currentCards[i], targetPositions[i]);
            }
        }

        UpdateCardDepths();
    }

    private IEnumerator AddCardAnimation(RectTransform newCard, CardType cardType)
    {
        List<RectTransform> currentCards = new List<RectTransform>(cards);
        float dynamicSpacing = CalculateDynamicSpacing();
        float totalWidth = (currentCards.Count - 1) * dynamicSpacing;
        Vector3 startPos = new Vector3(
            horizontalPosition.x - totalWidth / 2f,
            horizontalPosition.y,
            0
        );

        Vector3 targetPosition = cardType == CardType.LeftSide ?
            startPos :
            startPos + new Vector3((currentCards.Count - 1) * dynamicSpacing, 0, 0);

        Vector3 startPosition = cardType == CardType.LeftSide ?
            new Vector3(-Screen.width / 2, horizontalPosition.y, 0) :
            new Vector3(Screen.width / 2, horizontalPosition.y, 0);

        newCard.anchoredPosition = startPosition;

        Transform cardImage = newCard.Find("CardImage");
        Vector3 cardImageOriginalScale = Vector3.one;
        if (cardImage != null)
        {
            cardImageOriginalScale = cardImage.localScale;
            cardImage.localScale = cardImageOriginalScale * 0.8f;
        }
        else
        {
            newCard.localScale = Vector3.one * 0.8f;
        }

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);
            float curveProgress = moveCurve.Evaluate(progress);

            newCard.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, curveProgress);

            if (cardImage != null)
            {
                cardImage.localScale = Vector3.Lerp(
                    cardImageOriginalScale * 0.8f,
                    cardImageOriginalScale,
                    curveProgress
                );
            }
            else
            {
                newCard.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, curveProgress);
            }

            yield return null;
        }

        newCard.anchoredPosition = targetPosition;
        if (cardImage != null)
        {
            cardImage.localScale = cardImageOriginalScale;
        }
        else
        {
            newCard.localScale = Vector3.one;
        }

        originalPositions[newCard] = targetPosition;
        EnqueueAnimation(ArrangeCardsAnimationCoroutine());
    }

    void UpdateCardDepths()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetSiblingIndex(i);
        }
    }

    public void HoverCard(RectTransform card, bool isHovering)
    {
        if (IsAnyCardDragging || isAnyAnimationPlaying) return;
        CardCtrl cardCtrl = card.GetComponent<CardCtrl>();
        if (cardCtrl != null && cardCtrl.IsPlayingCard) return;

        if (isHovering && currentHoveredCard != null && currentHoveredCard != card)
        {
            ResetPreviousHoveredCard();
        }

        if (activeAnimations.TryGetValue(card, out Coroutine existingAnimation))
        {
            StopCoroutine(existingAnimation);
        }

        if (isHovering)
        {
            card.SetAsLastSibling();
            currentHoveredCard = card;
        }
        else
        {
            if (currentHoveredCard == card)
            {
                currentHoveredCard = null;
            }
            UpdateCardDepths();
        }

        activeAnimations[card] = StartCoroutine(HoverAnimation(card, isHovering));
    }

    private void ResetPreviousHoveredCard()
    {
        if (currentHoveredCard != null)
        {
            if (activeAnimations.TryGetValue(currentHoveredCard, out Coroutine existingAnimation))
            {
                StopCoroutine(existingAnimation);
                activeAnimations.Remove(currentHoveredCard);
            }

            StartCoroutine(HoverAnimation(currentHoveredCard, false));
            currentHoveredCard.localScale = Vector3.one;
            currentHoveredCard = null;
        }
    }

    IEnumerator HoverAnimation(RectTransform card, bool isHovering)
    {
        if (!originalPositions.ContainsKey(card))
        {
            originalPositions[card] = card.anchoredPosition;
        }

        Vector3 startPosition = card.anchoredPosition;
        Vector3 targetPosition = isHovering ?
            originalPositions[card] + new Vector3(0, hoverHeight, 0) :
            originalPositions[card];

        float timer = 0f;
        while (timer < hoverDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / hoverDuration);
            float curveProgress = moveCurve.Evaluate(progress);

            card.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, curveProgress);
            yield return null;
        }

        card.anchoredPosition = targetPosition;
        activeAnimations.Remove(card);
    }
}