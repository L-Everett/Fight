using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)] // 确保序列化字段而不是属性
public class DataModel
{
    private static readonly object _lock = new object();
    private static DataModel instance;

    [JsonProperty]
    public float mPlayTime;  // 游戏时间

    [JsonProperty]
    public int mGemCount;  // 天赋石数量

    [JsonProperty]
    public Dictionary<string, List<AttributeModifier>> mTalentAdd;

    [JsonProperty]
    public Dictionary<string, int> mTalentLv;

    public static DataModel Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new DataModel();
                        instance.Init(); // 确保初始化
                    }
                }
            }
            return instance;
        }
        private set { instance = value; }
    }

    public void Init()
    {
        mPlayTime = 0;
        mGemCount = 0;
        mTalentAdd = new Dictionary<string, List<AttributeModifier>>();
        mTalentLv = new Dictionary<string, int>();
    }

    public void LoadGame(DataModel dataModel)
    {
        if (dataModel == null)
        {
            Init();
            return;
        }

        mPlayTime = dataModel.mPlayTime;
        mGemCount = dataModel.mGemCount;

        mTalentAdd = dataModel.mTalentAdd ?? new Dictionary<string, List<AttributeModifier>>();
        mTalentLv = dataModel.mTalentLv ?? new Dictionary<string, int>();
    }

    public void GetGem(int count)
    {
        mGemCount += count;
        SaveSystem.SaveGame();
    }
}