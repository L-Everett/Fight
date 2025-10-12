using UnityEngine;

public class AddCard : MonoBehaviour
{
    public CardLayoutController mCL;
    public CardType mCardType;
    
    public void OnClick()
    {
        if (BattleManager.Instance.currentStage != BattleStage.DrawCrad)
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "非抽卡阶段!");
            return;
        }
        string id = BattleManager.Instance.AddCard(mCardType);
        if (id == string.Empty) return;
        mCL.AddCard(id);
        AudioManager.Instance.PlayDrawCard();
    }
}
