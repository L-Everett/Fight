using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [Header("Settings")]
    public GameObject damagePopupPrefab;
    public Vector3 defaultOffset = new Vector3(0, 2f, 0);

    private Queue<GameObject> popupPool = new Queue<GameObject>();
    private int poolSize = 10;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform);
            popup.SetActive(false);
            popupPool.Enqueue(popup);
        }
    }

    public void ShowDamage(Vector3 position, float damage, bool isCritical = false)
    {
        if (popupPool.Count == 0)
        {
            // 如果池空了，创建新对象
            GameObject newPopup = Instantiate(damagePopupPrefab, transform);
            popupPool.Enqueue(newPopup);
        }

        GameObject popup = popupPool.Dequeue();
        popup.transform.position = position + defaultOffset;
        popup.SetActive(true);

        DamagePopup damagePopup = popup.GetComponentInChildren<DamagePopup>();
        if (damagePopup != null)
        {
            damagePopup.SetDamage(damage, isCritical);
        }

        // 返回到池中
        StartCoroutine(ReturnToPool(popup));
    }

    public void ShowHeal(Vector3 position, float heal)
    {
        if (popupPool.Count == 0)
        {
            // 如果池空了，创建新对象
            GameObject newPopup = Instantiate(damagePopupPrefab, transform);
            popupPool.Enqueue(newPopup);
        }

        GameObject popup = popupPool.Dequeue();
        popup.transform.position = position + defaultOffset;
        popup.SetActive(true);

        DamagePopup damagePopup = popup.GetComponentInChildren<DamagePopup>();
        if (damagePopup != null)
        {
            damagePopup.SetHeal(heal);
        }

        // 返回到池中
        StartCoroutine(ReturnToPool(popup));
    }

    private IEnumerator ReturnToPool(GameObject popup)
    {
        // 等待动画完成
        DamagePopup damagePopup = popup.GetComponentInChildren<DamagePopup>();
        if (damagePopup != null)
        {
            yield return new WaitForSeconds(damagePopup.DestroyTime);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        popup.SetActive(false);
        popupPool.Enqueue(popup);
    }
}