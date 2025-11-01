using System.Collections;
using TMPro;
using UnityEngine;

public class TipUI : MonoBehaviour
{
    public TextMeshProUGUI mTipText;
    private float timer = 0f;
    public float lifeTime = 1.5f; // 总存活时间
    public System.Action OnFinish;

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;
    }

    public void InitUI(string content)
    {
        mTipText.text = content;
        timer = 0;
        StartCoroutine(UpFloat());
    }

    IEnumerator UpFloat()
    {
        while (timer < lifeTime)
        {
            // 每帧向上移动
            transform.localPosition += 50f * Time.deltaTime * Vector3.up;

            timer += Time.deltaTime;
            yield return null;
        }
        OnFinish?.Invoke();
    }
}
