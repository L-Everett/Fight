using UnityEngine;

public class TalentBtn : MonoBehaviour
{
    //高亮
    public Material normalMaterial; 
    public Material highlightMaterial; 
    private Renderer monumentRenderer; 

    //天赋树UI
    public GameObject talentRoot; 

    // 当鼠标在碰撞体上并点击时调用
    void OnMouseDown()
    {
        if (talentRoot != null)
        {
            talentRoot.SetActive(true);
            monumentRenderer.material = normalMaterial;
        }
    }

    void Start()
    {
        // 获取石碑上的 Renderer 组件（通常是 MeshRenderer）
        monumentRenderer = GetComponent<Renderer>();
        if (monumentRenderer == null)
        {
            Debug.LogError("Renderer component not found on the monument!");
        }
        monumentRenderer.material = normalMaterial;
    }

    void OnMouseEnter()
    {
        if (talentRoot != null && talentRoot.activeSelf) return;
        monumentRenderer.material = highlightMaterial;
        AudioManager.Instance.PlayHover();
    }

    void OnMouseExit()
    {
        monumentRenderer.material = normalMaterial;
    }
}