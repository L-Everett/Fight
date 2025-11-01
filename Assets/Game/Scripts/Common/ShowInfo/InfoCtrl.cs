using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InfoCtrl : MonoBehaviour
{
    [Header("组件引用")]
    public TextMeshProUGUI contentText;
    public Image backgroundImage;

    [Header("背景边距")]
    public Vector2 padding = new Vector2(20, 10);
    public float minWidth = 100f;
    public float maxWidth = 300f;

    public RectTransform backgroundRect;
    public RectTransform textRect;

    public void Init(string content)
    {
        if (contentText == null || backgroundRect == null || textRect == null)
        {
            Debug.LogError("InfoCtrl components are not properly set up!");
            return;
        }

        // 设置文本内容
        contentText.text = content;

        // 强制文本立即更新
        Canvas.ForceUpdateCanvases();
        contentText.ForceMeshUpdate();

        // 获取文本的实际尺寸
        Vector2 preferredSize = contentText.GetPreferredValues(content, maxWidth, 0f);

        // 计算背景尺寸
        float backgroundWidth = Mathf.Clamp(preferredSize.x + padding.x * 2, minWidth, maxWidth);
        float backgroundHeight = preferredSize.y + padding.y * 2;

        // 设置背景尺寸
        backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundWidth);
        backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundHeight);

        // 设置文本区域尺寸
        if (textRect != null)
        {
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundWidth - padding.x * 2);
        }

        // 再次强制更新确保尺寸正确
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
    }
}