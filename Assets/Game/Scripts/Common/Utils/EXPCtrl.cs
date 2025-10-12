using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EXPCtrl : MonoBehaviour
{
    public TextMeshProUGUI mLvText;
    public TextMeshProUGUI mExpText;
    public Image mExpFillImg;
    
    public void FreshUI(int curExp, int maxExp, int lv)
    {
        mLvText.text = "Lv." + lv;
        mExpText.text = curExp + "/" + maxExp;
        mExpFillImg.fillAmount = (float)curExp / maxExp;
    }
}
