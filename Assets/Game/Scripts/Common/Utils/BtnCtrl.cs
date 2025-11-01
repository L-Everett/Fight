using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnCtrl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image mBtnBg;
    public TextMeshProUGUI mText;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            mBtnBg.color = Color.white;
            mText.color = Color.black;
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mBtnBg.color = Color.black;
        mText.color = Color.white;
        AudioManager.Instance.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mBtnBg.color = Color.white;
        mText.color = Color.black;
    }
}
