using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FireOutlineController : MonoBehaviour
{
    private Material outlineMaterial;
    private Image targetImage;

    [Header("火焰参数")]
    public CardType mCardType;
    public float minWidth = 1f;
    public float maxWidth = 5f;
    public float pulseSpeed = 2f;

    private Color[] rightCardColors = {
        new Color(1f, 0.022f, 0f),
        new Color(1f, 0.23f, 0.33f),
        new Color(1f, 0.53f, 0.52f)
    };

    private Color[] leftCardColors = {
        new Color(0f, 0.085f, 1f),
        new Color(0.25f, 0.32f, 1f),
        new Color(0.48f, 0.53f, 1f)
    };

    private List<Color> fireColors = new List<Color>();

    void Start()
    {
        targetImage = GetComponent<Image>();
        outlineMaterial = Instantiate(targetImage.material);
        targetImage.material = outlineMaterial;
        FreshColor();
    }

    public void FreshColor()
    {
        fireColors.Clear();
        if (mCardType == CardType.LeftSide)
        {
            foreach (Color c in leftCardColors)
            {
                fireColors.Add(c);
            }
        }
        else
        {
            foreach (Color c in rightCardColors)
            {
                fireColors.Add(c);
            }
        }
    }

    void Update()
    {
        // 动态描边宽度（脉动效果）
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        float width = Mathf.Lerp(minWidth, maxWidth, pulse);
        outlineMaterial.SetFloat("_OutlineWidth", width);

        // 动态火焰颜色（循环渐变）
        float colorPhase = Mathf.PingPong(Time.time * 0.5f, 1f);
        Color currentColor = ColorHSL.Lerp(
            fireColors[0],
            fireColors[1],
            fireColors[2],
            colorPhase
        );

        outlineMaterial.SetColor("_OutlineColor", currentColor);

        // 设置火焰强度波动
        float intensity = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
        outlineMaterial.SetFloat("_FireIntensity", intensity);
    }

    // 外部控制火焰速度
    public void SetFireSpeed(float speed)
    {
        outlineMaterial.SetFloat("_FireSpeed", Mathf.Clamp(speed, 0.1f, 5f));
    }

    // 外部控制火焰扰动强度
    public void SetNoiseScale(float scale)
    {
        outlineMaterial.SetFloat("_NoiseScale", Mathf.Clamp(scale, 0f, 0.1f));
    }
}

public static class ColorHSL
{
    public static Color Lerp(Color a, Color b, Color c, float t)
    {
        if (t < 0.5f)
        {
            return Color.Lerp(a, b, t * 2f);
        }
        else
        {
            return Color.Lerp(b, c, (t - 0.5f) * 2f);
        }
    }
}