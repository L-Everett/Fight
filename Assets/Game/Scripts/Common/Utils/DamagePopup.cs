using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float destroyTime = 1f;
    [HideInInspector] public float DestroyTime { get { return destroyTime; }}

    [Header("Visual Settings")]
    public float floatSpeed = 1f;          // 上浮速度
    public float floatHeight = 1f;         // 上浮高度
    public float fadeDuration = 0.5f;      // 淡出持续时间

    private Vector3 startPosition;         // 初始位置
    private float startTime;               // 动画开始时间

    private Color initColor;
    private float initFontSize;

    private void Awake()
    {
        // 自动获取Text组件
        if (damageText == null)
        {
            damageText = GetComponentInChildren<TextMeshProUGUI>();
        }

        initColor = damageText.color;
        initFontSize = damageText.fontSize;
    }

    private void OnEnable()
    {
        startPosition = transform.position;
        damageText.color = initColor;
        damageText.fontSize = initFontSize;
    }

    // 设置伤害值
    public void SetDamage(float damage, bool isCritical = false)
    {
        if (damageText == null) return;

        // 设置文本
        damageText.text = "-" + Mathf.RoundToInt(damage).ToString();

        //淡出
        StartCoroutine(FloatAndFadeRoutine());

        // 暴击效果
        if (isCritical)
        {
            damageText.fontSize *= 1.5f;
            damageText.color = Color.yellow;

            // 添加抖动动画
            StartCoroutine(PunchScale(damageText.gameObject, 1.2f, 0.2f));
        }
    }

    //治疗值
    public void SetHeal(float heal)
    {
        if (damageText == null) return;

        // 设置文本
        damageText.text = "+" + Mathf.RoundToInt(heal).ToString();
        damageText.color = Color.green;

        //淡出
        StartCoroutine(FloatAndFadeRoutine());
    }

    IEnumerator PunchScale(GameObject target, float scaleMultiplier, float duration)
    {
        Vector3 originalScale = target.transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // 弹性公式
            float overshoot = 1.3f * Mathf.Sin(t * Mathf.PI * 2.5f) * (1 - t);
            float currentScale = Mathf.Lerp(1, scaleMultiplier, t) + overshoot;

            target.transform.localScale = originalScale * currentScale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.transform.localScale = targetScale;
    }

    private IEnumerator FloatAndFadeRoutine()
    {
        startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < destroyTime)
        {
            elapsedTime = Time.time - startTime;
            float progress = elapsedTime / destroyTime;

            // 计算新位置（向上浮动）
            float newY = startPosition.y + Mathf.Lerp(0, floatHeight, progress);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 计算透明度（最后fadeDuration秒开始淡出）
            float fadeProgress = Mathf.Clamp01((elapsedTime - (destroyTime - fadeDuration)) / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, fadeProgress);

            // 应用透明度
            Color newColor = damageText.color;
            newColor.a = alpha;
            damageText.color = newColor;

            yield return null;
        }
    }
}