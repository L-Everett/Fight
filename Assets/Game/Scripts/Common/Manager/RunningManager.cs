
using System.Collections.Generic;

public class RunningManager
{
    private static readonly object _lock = new object();
    private static RunningManager instance;
    public static RunningManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance = new RunningManager();
                }
            }
            return instance;
        }
        private set { }
    }

    public string mCurrentCharacter = "C001";
    public int mCurrentDiff = 0;
    public int mMaxRound = 10;
    public Dictionary<string, Dictionary<AttributeType, float>> mTalentAdd = new Dictionary<string, Dictionary<AttributeType, float>>();
}