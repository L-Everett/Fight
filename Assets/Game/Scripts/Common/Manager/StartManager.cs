using UnityEngine;


public class StartManager : MonoBehaviour
{
    public GameObject setting;
    public GameObject select;
    public GameObject environment;

    private void OnEnable()
    {
        environment.SetActive(true);
        select.SetActive(false);
    }

    public void StartGame()
    {
        select.SetActive(true);
        environment.SetActive(false);
        gameObject.SetActive(false);
    }

    public void StartSetting()
    {
        setting.SetActive(true);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止运行
#else
            Application.Quit(); // 在构建版本中退出应用
#endif
    }
}
