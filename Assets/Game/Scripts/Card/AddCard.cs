using UnityEngine;

public class AddCard : MonoBehaviour
{
    public CardLayoutController mCL;
    public CardType mCardType;
    
    public void OnClick()
    {
        if (BattleManager.Instance.currentStage != BattleStage.DoCard)
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "非打牌阶段!");
            return;
        }
        string id = BattleManager.Instance.AddCard(mCardType);
        if (id == string.Empty) return;
        mCL.AddCard(id);
        AudioManager.Instance.PlayDrawCard();
    }
}
