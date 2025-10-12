using UnityEngine;
using UnityEngine.EventSystems;

public class CardScaleController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float mInitScale;
    public float mSelectScale;
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(mSelectScale, mSelectScale, mSelectScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(mInitScale, mInitScale, mInitScale);
    }
}
