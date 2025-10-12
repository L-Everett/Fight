using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TipCtrl : MonoBehaviour, IMsgHandler
{
    public GameObject mTip;
    private Queue<string> messageQueue = new Queue<string>();
    private Stack<TipUI> tipPool = new Stack<TipUI>();
    private bool isProcessing = false;

    private void Start()
    {
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_TIP_SHOW, this);
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
        StopAllCoroutines();
        messageQueue.Clear();
        tipPool.Clear();
    }

    public void Handle(string msg, object obj)
    {
        if (msg == Constant.MSG_NOTIFY_TIP_SHOW)
        {
            messageQueue.Enqueue((string)obj);
            if (!isProcessing)
            {
                StartCoroutine(ProcessMessageQueue());
            }
        }
    }

    private IEnumerator ProcessMessageQueue()
    {
        isProcessing = true;
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            TipUI tip = GetTipFromPool();
            tip.InitUI(message);
            yield return new WaitForSeconds(0.5f);
        }
        isProcessing = false;
    }

    private TipUI GetTipFromPool()
    {
        TipUI tip;
        if (tipPool.Count > 0)
        {
            tip = tipPool.Pop();
            tip.gameObject.SetActive(true);
        }
        else
        {
            tip = Instantiate(mTip, transform).GetComponent<TipUI>();
            tip.OnFinish += () => ReturnTipToPool(tip);
        }
        return tip;
    }

    private void ReturnTipToPool(TipUI tip)
    {
        tip.gameObject.SetActive(false);
        tipPool.Push(tip);
    }
}