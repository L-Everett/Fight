using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPCtrl : MonoBehaviour
{
    public Image mFillImg;
    public Image mFillImgEffect;
    public TextMeshProUGUI mVaslueText;
    private Sequence mSequence;

    public void FreshHp(float currentValue, float maxValue)
    {
        if (mFillImg == null) return;
        if (mSequence == null)
        {
            mSequence = DOTween.Sequence();
        }
        mSequence.Append(mFillImg.DOFillAmount(currentValue / maxValue, 0.3f));
        mSequence.Join(mFillImgEffect.DOFillAmount(currentValue / maxValue, 0.9f));
        if (mVaslueText != null)
        {
            mVaslueText.text = currentValue.ToString("F0") + "/" + maxValue.ToString("F0");
        }
    }

    private void OnDestroy()
    {
        if(mSequence != null)
        {
            mSequence.Kill();
            mSequence = null;
        }
    }
}
