using System;
using UnityEngine;

[Serializable]
public class DataModel
{
    private static readonly object _lock = new object();
    private static DataModel instance;

    //游戏内数据
    public int mPlayerMaxHp;  //最大血量
    public int mPlayerCurHp;  //当前血量
    public Vector3 mPlayerPosition;  //玩家位置
    public float mPlayTime;  //游戏时间
    public int mUnlockLevelCount;  //解锁关卡
    public int mGoldCount;  //灵石数量
    public int mTanentGoldCount;  //天赋石数量

    public static DataModel Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance = new DataModel();
                }
            }
            return instance;
        }
        private set { }
    }

    public void Init()
    {

        mPlayerPosition = new Vector3(0, 0, 0);
        mPlayTime = 0;
        mUnlockLevelCount = 1;
        mGoldCount = 0;
        mTanentGoldCount = 0;
}

    public void SaveGame()
    {

    }

    public void LoadGame(DataModel dataModel)
    {
        mPlayerPosition = dataModel.mPlayerPosition;
        mPlayerMaxHp = dataModel.mPlayerMaxHp;
        mPlayerCurHp = dataModel.mPlayerCurHp;
        mPlayTime = dataModel.mPlayTime;
        mUnlockLevelCount = dataModel.mUnlockLevelCount;
        mGoldCount = dataModel.mGoldCount;
        mTanentGoldCount = dataModel.mTanentGoldCount;
    }

}
