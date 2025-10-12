using System.Collections.Generic;

public class StaticEnemy
{
    public Dictionary<string, Static_Enemy_t> mDatas = new Dictionary<string, Static_Enemy_t>();
    public bool Init()
    {
        CSVReader reader = StaticDataInterface.LoadCSV("Enemy");
        int iRowCount = reader.GetRowCount();
        mDatas = new Dictionary<string, Static_Enemy_t>();
        for (int i = 0; i < iRowCount; i++)
        {
            Dictionary<string, string> data = reader.GetRow(i);
            Static_Enemy_t info = new Static_Enemy_t(ref data);
            if (info.Id == string.Empty) continue;
            mDatas.Add(info.Id, info);
        }
        return true;
    }

    public void DeInit()
    {
        mDatas.Clear();
    }

    public int GetCount() { return mDatas.Count; }

    public Static_Enemy_t GetEnemy(string id)
    {
        mDatas.TryGetValue(id, out Static_Enemy_t static_Enemy_T);
        return static_Enemy_T;
    }

    public List<string> GetAllIds()
    {
        List<string> list = new List<string>();
        foreach (string id in mDatas.Keys)
        {
            list.Add(id);
        }
        return list;
    }
}
