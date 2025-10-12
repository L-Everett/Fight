using UnityEngine;


public class ExitManager : MonoBehaviour
{
    public void YES()
    {
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SCENE_CHANGE, "StartScene");
    }

    public void NO()
    {
        BattleManager.Instance.isOnESC = false;
        BattleManager.Instance.Resume();
        gameObject.SetActive(false);
    }
}
