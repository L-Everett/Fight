using System.Collections.Generic;

public class StaticRound
{
    public Dictionary<int, Static_Round_t> mDatas = new Dictionary<int, Static_Round_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Round");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<int, Static_Round_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Round_t info = new Static_Round_t(ref data);
            //if (info.Id == string.Empty) continue;
            mDatas.Add(info.Id, info);
        }
        return true;
    }

    public void DeInit()
    {
        mDatas.Clear();
    }

    public int GetCount() { return mDatas.Count; }

    public Static_Round_t GetRound(int id)
    {
        mDatas.TryGetValue(id, out Static_Round_t static_Round_T);
        return static_Round_T;
    }
}
