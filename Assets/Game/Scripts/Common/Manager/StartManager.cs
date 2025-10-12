using UnityEngine;


public class StartManager : MonoBehaviour
{
    public GameObject setting;

    public void StartGame()
    {
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SCENE_CHANGE, "MainScene");
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
