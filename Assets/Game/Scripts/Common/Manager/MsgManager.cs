using System.Collections.Generic;
using UnityEngine;

public interface IMsgHandler
{
    //public void Handle(string msg);

    public void Handle(string msg, object obj);
}

public class MsgManager
{
    private static MsgManager _instance;
    private static readonly object _lock = new object();
    private Dictionary<string, List<IMsgHandler>> mMsgDic = new Dictionary<string, List<IMsgHandler>>();

    public static MsgManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = new MsgManager();
                }
            }
            return _instance;
        }
    }

    public void EmitMsg(string msg)
    {
        if (mMsgDic.ContainsKey(msg))
        {
            var handlerList = mMsgDic[msg];
            foreach (var handler in handlerList)
            {
                handler.Handle(msg, null);
            }
        }
    }

    public void EmitMsg(string msg, object obj)
    {
        if (mMsgDic.ContainsKey(msg))
        {
            var handlerList = mMsgDic[msg];
            foreach (var handler in handlerList)
            {
                handler.Handle(msg, obj);
            }
        }
        else
        {
            Debug.Log("No handler");
        }
    }

    public void AddMsgListener(string msg, IMsgHandler handler)
    {
        if (!mMsgDic.ContainsKey(msg))
        {
            mMsgDic.Add(msg, new List<IMsgHandler>());
        }
        if (!mMsgDic[msg].Contains(handler))
        {
            mMsgDic[msg].Add(handler);
        }
    }

    public void RemoveALLMsgListener(IMsgHandler handler)
    {
        var msgDic = mMsgDic.Values;
        foreach (var list in msgDic)
        {
            list.Remove(handler);
        }
    }
}
